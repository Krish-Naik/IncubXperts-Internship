namespace BankApi.Domain.Entities;

public class BankAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AccountNumber { get; set; } = default!;
    public string HolderName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Type { get; set; } = default!;
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}