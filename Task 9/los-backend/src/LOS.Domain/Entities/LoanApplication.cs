using LOS.Domain.Enums;

namespace LOS.Domain.Entities;

public class LoanApplication
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public InternalUser Customer { get; set; } = null!;
    public Guid? BrokerId { get; set; }
    public InternalUser? Broker { get; set; }
    public LoanType LoanType { get; set; }
    public decimal RequestedAmount { get; set; }
    public int RequestedTenureMonths { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;
    public decimal? ApprovedInterestRate { get; set; }
    public int? ApprovedTenureMonths { get; set; }
    public decimal? MonthlyEmi { get; set; }
    public string? RejectionReason { get; set; }

    public string? InfoRequestDetails { get; set; }
    public string? InfoResponseText { get; set; }

    public string? ExtraDetailsJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? DisbursedAtUtc { get; set; }

    public ICollection<CoApplicant> CoApplicants { get; set; } = [];
    public ICollection<KycDocument> KycDocuments { get; set; } = [];
    public ICollection<ApplicationAuditLog> AuditLogs { get; set; } = [];
}
