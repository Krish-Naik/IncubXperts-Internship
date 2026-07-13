namespace BankApi.Domain.Entities;

public sealed class AppUser
{
    public Guid Id { get; set; }
    public string EntraObjectId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
}
