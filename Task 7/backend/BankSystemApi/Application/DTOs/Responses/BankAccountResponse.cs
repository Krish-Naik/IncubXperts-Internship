namespace BankApi.Application.DTOs.Responses;

public class BankAccountResponse
{
    public Guid Id { get; set; }

    public Guid AppUserId { get; set; }

    public string AccountNumber { get; set; } = default!;
    public string HolderName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Type { get; set; } = default!;
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
