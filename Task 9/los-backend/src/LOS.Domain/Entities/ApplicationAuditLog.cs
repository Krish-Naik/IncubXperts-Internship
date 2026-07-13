namespace LOS.Domain.Entities;

public class ApplicationAuditLog
{
    public Guid Id { get; set; }
    public Guid LoanApplicationId { get; set; }
    public LoanApplication LoanApplication { get; set; } = null!;
    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
