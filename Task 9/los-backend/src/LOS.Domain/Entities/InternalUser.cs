using LOS.Domain.Enums;

namespace LOS.Domain.Entities;

public class InternalUser
{
    public Guid Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Invited;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public AuthUser? Auth { get; set; }
}
