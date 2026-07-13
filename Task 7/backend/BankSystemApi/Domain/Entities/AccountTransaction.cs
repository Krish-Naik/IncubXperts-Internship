namespace BankApi.Domain.Entities;

public sealed class AccountTransaction
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;

    public Guid? RelatedAccountId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;

    public Guid? PerformedByUserId { get; set; }
    public string PerformedByRole { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
