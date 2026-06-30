namespace LOS.Domain.Entities;

public class AuthUser
{
    public Guid Id { get; set; }
    public Guid InternalUserId { get; set; }
    public InternalUser InternalUser { get; set; } = null!;
    public string PasswordHash { get; set; } = string.Empty;
    public bool MustChangePassword { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public ICollection<PasswordHistory> PasswordHistory { get; set; } = [];
}
