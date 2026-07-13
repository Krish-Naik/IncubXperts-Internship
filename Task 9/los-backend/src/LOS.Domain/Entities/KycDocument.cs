using LOS.Domain.Enums;

namespace LOS.Domain.Entities;

public class KycDocument
{
    public Guid Id { get; set; }
    public Guid LoanApplicationId { get; set; }
    public LoanApplication LoanApplication { get; set; } = null!;
    public DocumentType DocType { get; set; }
    public string StorageProvider { get; set; } = "LocalDisk";
    public string StoragePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.PendingReview;
    public string? ReviewerRemarks { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public bool UploadedByBroker { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAtUtc { get; set; }
    public bool IsArchived { get; set; }
}
