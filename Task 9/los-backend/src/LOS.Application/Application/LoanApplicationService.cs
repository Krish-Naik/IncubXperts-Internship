using LOS.Application.Email;
using LOS.Domain.Entities;
using LOS.Domain.Enums;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Applications;

public class LoanApplicationService(LOSDbContext db, IEmailService emailService)
{
    public async Task<LoanApplication> CreateDraftAsync(
        Guid customerId,
        LoanType loanType,
        decimal amount,
        int tenureMonths,
        Dictionary<string, string>? extraDetails,
        CancellationToken ct
    )
    {
        var customer =
            await db.InternalUsers.FirstOrDefaultAsync(u => u.Id == customerId, ct)
            ?? throw new InvalidOperationException("Customer not found.");

        var app = new LoanApplication
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            BrokerId = customer.ReferredByBrokerId,
            LoanType = loanType,
            RequestedAmount = amount,
            RequestedTenureMonths = tenureMonths,
            Status = ApplicationStatus.Draft,
            ReferenceNumber = $"LOS-{DateTime.UtcNow.Year}-{Random.Shared.Next(100000, 999999)}",
            ExtraDetailsJson = extraDetails is { Count: > 0 }
                ? System.Text.Json.JsonSerializer.Serialize(extraDetails)
                : null,
        };
        db.LoanApplications.Add(app);
        await db.SaveChangesAsync(ct);
        return app;
    }

    public async Task<LoanApplication> SubmitAsync(
        Guid applicationId,
        Guid customerId,
        CancellationToken ct
    )
    {
        var app =
            await db
                .LoanApplications.Include(a => a.KycDocuments)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.CustomerId == customerId, ct)
            ?? throw new InvalidOperationException("Application not found.");

        if (!app.KycDocuments.Any(d => !d.IsArchived))
            throw new InvalidOperationException(
                "At least one KYC document is required before submission."
            );

        app.Status = ApplicationStatus.Submitted;
        app.SubmittedAtUtc = DateTime.UtcNow;
        app.UpdatedAtUtc = DateTime.UtcNow;

        db.ApplicationAuditLogs.Add(
            new ApplicationAuditLog
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = app.Id,
                ActorUserId = customerId,
                Action = "StatusChanged",
                OldValue = "Draft",
                NewValue = "Submitted",
            }
        );

        await db.SaveChangesAsync(ct);

        var customer = await db.InternalUsers.FirstAsync(u => u.Id == customerId, ct);
        await emailService.SendAsync(
            customer.Email,
            $"Application {app.ReferenceNumber} Submitted",
            $"<p>Your loan application <b>{app.ReferenceNumber}</b> has been submitted and is now under review.</p>",
            ct
        );

        return app;
    }

    public async Task<LoanApplication> RespondToInfoRequestAsync(
        Guid applicationId,
        Guid customerId,
        string responseText,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(responseText))
            throw new InvalidOperationException("Enter a response before resubmitting.");

        var app =
            await db.LoanApplications.FirstOrDefaultAsync(
                a => a.Id == applicationId && a.CustomerId == customerId,
                ct
            ) ?? throw new InvalidOperationException("Application not found.");

        if (app.Status != ApplicationStatus.InfoRequested)
            throw new InvalidOperationException(
                "This application is not currently awaiting additional information."
            );

        app.InfoResponseText = responseText;
        app.Status = ApplicationStatus.UnderReview;
        app.UpdatedAtUtc = DateTime.UtcNow;

        db.ApplicationAuditLogs.Add(
            new ApplicationAuditLog
            {
                Id = Guid.NewGuid(),
                LoanApplicationId = app.Id,
                ActorUserId = customerId,
                Action = "InfoRequestResponded",
                OldValue = "InfoRequested",
                NewValue = "UnderReview",
            }
        );

        await db.SaveChangesAsync(ct);

        var customer = await db.InternalUsers.FirstAsync(u => u.Id == customerId, ct);
        await emailService.SendAsync(
            customer.Email,
            $"Application {app.ReferenceNumber} Resubmitted",
            $"<p>Thanks — we've received your response and your application <b>{app.ReferenceNumber}</b> is back under review.</p>",
            ct
        );

        return app;
    }

    public async Task<CoApplicant> AddCoApplicantAsync(
        Guid applicationId,
        Guid customerId,
        string fullName,
        string pan,
        decimal monthlyIncome,
        CancellationToken ct
    )
    {
        var app =
            await db.LoanApplications.FirstOrDefaultAsync(
                a => a.Id == applicationId && a.CustomerId == customerId,
                ct
            ) ?? throw new InvalidOperationException("Application not found.");

        // Only editable while the application hasn't left the customer's hands.
        if (app.Status is not (ApplicationStatus.Draft or ApplicationStatus.InfoRequested))
            throw new InvalidOperationException(
                "Co-applicants can only be added while the application is a draft or awaiting your response."
            );

        var coApplicant = new CoApplicant
        {
            Id = Guid.NewGuid(),
            LoanApplicationId = applicationId,
            FullName = fullName.Trim(),
            Pan = pan.Trim().ToUpperInvariant(),
            MonthlyIncome = monthlyIncome,
        };
        db.CoApplicants.Add(coApplicant);
        await db.SaveChangesAsync(ct);
        return coApplicant;
    }

    public async Task RemoveCoApplicantAsync(
        Guid applicationId,
        Guid coApplicantId,
        Guid customerId,
        CancellationToken ct
    )
    {
        var app =
            await db.LoanApplications.FirstOrDefaultAsync(
                a => a.Id == applicationId && a.CustomerId == customerId,
                ct
            ) ?? throw new InvalidOperationException("Application not found.");

        if (app.Status is not (ApplicationStatus.Draft or ApplicationStatus.InfoRequested))
            throw new InvalidOperationException(
                "Co-applicants can only be removed while the application is a draft or awaiting your response."
            );

        var coApplicant =
            await db.CoApplicants.FirstOrDefaultAsync(
                c => c.Id == coApplicantId && c.LoanApplicationId == applicationId,
                ct
            ) ?? throw new InvalidOperationException("Co-applicant not found.");

        db.CoApplicants.Remove(coApplicant);
        await db.SaveChangesAsync(ct);
    }

    public record CoApplicantDto(Guid Id, string FullName, string Pan, decimal MonthlyIncome);

    public record ApplicationDocumentDto(
        Guid Id,
        DocumentType DocType,
        DocumentStatus Status,
        string OriginalFileName,
        bool UploadedByBroker,
        DateTime UploadedAtUtc,
        string? ReviewerRemarks
    );

    public record ApplicationDetailDto(
        Guid Id,
        string ReferenceNumber,
        ApplicationStatus Status,
        LoanType LoanType,
        decimal RequestedAmount,
        int RequestedTenureMonths,
        DateTime CreatedAtUtc,
        DateTime? SubmittedAtUtc,
        string CustomerName,
        string CustomerEmail,
        string CustomerPhone,
        string? BrokerName,
        Dictionary<string, string> ExtraDetails,
        List<CoApplicantDto> CoApplicants,
        List<ApplicationDocumentDto> Documents,
        string? InfoRequestDetails,
        string? InfoResponseText,
        decimal? ApprovedInterestRate,
        int? ApprovedTenureMonths,
        decimal? MonthlyEmi,
        string? RejectionReason
    );

    public async Task<ApplicationDetailDto> GetApplicationDetailAsync(
        Guid applicationId,
        CancellationToken ct
    )
    {
        var app =
            await db
                .LoanApplications.Include(a => a.Customer)
                .Include(a => a.Broker)
                .Include(a => a.CoApplicants)
                .Include(a => a.KycDocuments)
                .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new InvalidOperationException("Application not found.");

        var extraDetails = string.IsNullOrWhiteSpace(app.ExtraDetailsJson)
            ? new Dictionary<string, string>()
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(
                app.ExtraDetailsJson
            ) ?? new Dictionary<string, string>();

        return new ApplicationDetailDto(
            app.Id,
            app.ReferenceNumber,
            app.Status,
            app.LoanType,
            app.RequestedAmount,
            app.RequestedTenureMonths,
            app.CreatedAtUtc,
            app.SubmittedAtUtc,
            app.Customer.FullName,
            app.Customer.Email,
            app.Customer.Phone,
            app.Broker?.FullName,
            extraDetails,
            app.CoApplicants.Select(c => new CoApplicantDto(
                    c.Id,
                    c.FullName,
                    c.Pan,
                    c.MonthlyIncome
                ))
                .ToList(),
            app.KycDocuments.Where(d => !d.IsArchived)
                .OrderByDescending(d => d.UploadedAtUtc)
                .Select(d => new ApplicationDocumentDto(
                    d.Id,
                    d.DocType,
                    d.Status,
                    d.OriginalFileName,
                    d.UploadedByBroker,
                    d.UploadedAtUtc,
                    d.ReviewerRemarks
                ))
                .ToList(),
            app.InfoRequestDetails,
            app.InfoResponseText,
            app.ApprovedInterestRate,
            app.ApprovedTenureMonths,
            app.MonthlyEmi,
            app.RejectionReason
        );
    }

    public Task<List<LoanApplication>> GetMyApplicationsAsync(
        Guid customerId,
        CancellationToken ct
    ) =>
        db
            .LoanApplications.Include(a => a.CoApplicants)
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(ct);
}
