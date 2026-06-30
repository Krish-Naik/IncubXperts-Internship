namespace LOS.Domain.Entities;

public class PasswordHistory
{
    public Guid Id { get; set; }
    public Guid AuthUserId { get; set; }
    public AuthUser AuthUser { get; set; } = null!;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
