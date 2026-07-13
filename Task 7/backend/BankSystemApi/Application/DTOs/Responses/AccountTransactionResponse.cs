namespace BankApi.Application.DTOs.Responses;

public sealed class AccountTransactionResponse
{
    public Guid Id { get; set; }
    public Guid BankAccountId { get; set; }
    public Guid? RelatedAccountId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PerformedByRole { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
