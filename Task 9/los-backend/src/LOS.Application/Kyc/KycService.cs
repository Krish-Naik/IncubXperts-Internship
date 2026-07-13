using LOS.Application.Email;
using LOS.Application.Storage;
using LOS.Domain.Entities;
using LOS.Domain.Enums;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Kyc;

public class KycService(
    LOSDbContext db,
    IFileStorageService storage,
    IFileStorageResolver storageResolver,
    IEmailService emailService
)
{
    private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
    private const long MaxSizeBytes = 5 * 1024 * 1024;

    public async Task<KycDocument> UploadAsync(
        Guid applicationId,
        DocumentType docType,
        Stream content,
        string fileName,
        long sizeBytes,
        bool uploadedByBroker,
        CancellationToken ct
    )
    {
        var app =
            await db.LoanApplications.FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new InvalidOperationException("Application not found.");

        if (app.Status is not (ApplicationStatus.Draft or ApplicationStatus.InfoRequested))
            throw new InvalidOperationException(
                "Documents can only be uploaded while the application is a draft or awaiting your response to an information request."
            );

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("Unsupported file type. Allowed: PDF, JPG, PNG.");
        if (sizeBytes > MaxSizeBytes)
            throw new InvalidOperationException("File exceeds 5MB limit.");

        var existing = await db
            .KycDocuments.Where(d =>
                d.LoanApplicationId == applicationId && d.DocType == docType && !d.IsArchived
            )
            .FirstOrDefaultAsync(ct);
        if (existing != null)
            existing.IsArchived = true;

        var stored = await storage.SaveAsync(content, fileName, $"kyc/{applicationId}", ct);

        var doc = new KycDocument
        {
            Id = Guid.NewGuid(),
            LoanApplicationId = applicationId,
            DocType = docType,
            StorageProvider = stored.StorageProvider,
            StoragePath = stored.StoragePath,
            OriginalFileName = fileName,
            UploadedByBroker = uploadedByBroker,
        };
        db.KycDocuments.Add(doc);
        await db.SaveChangesAsync(ct);
        return doc;
    }

    public async Task ReviewAsync(
        Guid documentId,
        Guid officerId,
        DocumentStatus status,
        string? remarks,
        CancellationToken ct
    )
    {
        var doc =
            await db
                .KycDocuments.Include(d => d.LoanApplication)
                    .ThenInclude(a => a.Customer)
                .FirstOrDefaultAsync(d => d.Id == documentId, ct)
            ?? throw new InvalidOperationException("Document not found.");

        if (status == DocumentStatus.Rejected && string.IsNullOrWhiteSpace(remarks))
            throw new InvalidOperationException("Remarks are mandatory when rejecting a document.");

        doc.Status = status;
        doc.ReviewerRemarks = remarks;
        doc.ReviewedByUserId = officerId;
        doc.ReviewedAtUtc = DateTime.UtcNow;

        var app = doc.LoanApplication;
        var previousStatus = app.Status;

        // First review action moves the application out of the submitted queue
        // and into "being worked on".
        if (app.Status == ApplicationStatus.Submitted)
        {
            app.Status = ApplicationStatus.UnderReview;
            app.UpdatedAtUtc = DateTime.UtcNow;
        }

        if (status == DocumentStatus.Rejected)
        {
            // A rejected KYC document blocks the application outright - this is what
            // makes the application actually show "Rejected" everywhere (customer
            // portal, verification queue, manager queue) instead of silently staying
            // "UnderReview" forever.
            app.Status = ApplicationStatus.Rejected;
            app.RejectionReason = $"{doc.DocType} document rejected: {remarks}";
            app.UpdatedAtUtc = DateTime.UtcNow;

            db.ApplicationAuditLogs.Add(
                new ApplicationAuditLog
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = app.Id,
                    ActorUserId = officerId,
                    Action = "DocumentRejected",
                    OldValue = previousStatus.ToString(),
                    NewValue = ApplicationStatus.Rejected.ToString(),
                }
            );
        }
        else
        {
            // Once every required document type is Approved, the application is
            // ready for a Branch Manager to act on.
            var requiredTypes = new[] { DocumentType.Aadhaar, DocumentType.Pan };
            var docsForApp = await db
                .KycDocuments.Where(d => d.LoanApplicationId == app.Id && !d.IsArchived)
                .ToListAsync(ct);

            var allApproved = requiredTypes.All(t =>
                docsForApp.Any(d => d.DocType == t && d.Status == DocumentStatus.Approved)
            );

            if (
                allApproved
                && app.Status is ApplicationStatus.UnderReview or ApplicationStatus.InfoRequested
            )
            {
                app.Status = ApplicationStatus.Verified;
                app.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(ct);

        if (status == DocumentStatus.Rejected)
        {
            await emailService.SendAsync(
                app.Customer.Email,
                $"Application {app.ReferenceNumber} - Document Rejected",
                $"<p>Your <b>{doc.DocType}</b> document could not be verified and your application "
                    + $"<b>{app.ReferenceNumber}</b> has been rejected.</p>"
                    + $"<p><b>Reason:</b> {remarks}</p>"
                    + "<p>Please log in to your account for details, or contact your branch for help "
                    + "with a fresh application.</p>",
                ct
            );
        }
    }

    public record KycQueueItemDto(
        Guid Id,
        Guid LoanApplicationId,
        string ReferenceNumber,
        string CustomerName,
        DocumentType DocType,
        string OriginalFileName,
        bool UploadedByBroker,
        DateTime UploadedAtUtc
    );

    public Task<List<KycQueueItemDto>> GetReviewQueueAsync(CancellationToken ct) =>
        db
            .KycDocuments.Where(d => !d.IsArchived && d.Status == DocumentStatus.PendingReview)
            .OrderBy(d => d.UploadedAtUtc)
            .Select(d => new KycQueueItemDto(
                d.Id,
                d.LoanApplicationId,
                d.LoanApplication.ReferenceNumber,
                d.LoanApplication.Customer.FullName,
                d.DocType,
                d.OriginalFileName,
                d.UploadedByBroker,
                d.UploadedAtUtc
            ))
            .ToListAsync(ct);

    public record DocumentFile(Stream Content, string FileName, string ContentType);

    public async Task<DocumentFile> GetDocumentFileAsync(Guid documentId, CancellationToken ct)
    {
        var doc =
            await db.KycDocuments.FirstOrDefaultAsync(d => d.Id == documentId, ct)
            ?? throw new InvalidOperationException("Document not found.");

        var resolvedStorage = storageResolver.Resolve(doc.StorageProvider);
        var content = await resolvedStorage.GetAsync(doc.StoragePath, ct);
        var contentType = Path.GetExtension(doc.OriginalFileName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream",
        };
        return new DocumentFile(content, doc.OriginalFileName, contentType);
    }
}
