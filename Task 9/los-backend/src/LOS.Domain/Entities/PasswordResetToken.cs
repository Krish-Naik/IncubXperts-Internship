namespace LOS.Domain.Entities;

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid InternalUserId { get; set; }
    public InternalUser InternalUser { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }
    public bool IsInvite { get; set; }
    public bool IsValid => UsedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}
