namespace LOS.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid InternalUserId { get; set; }
    public InternalUser InternalUser { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}
