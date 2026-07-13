namespace LOS.Application.Users.DTOs;

public record CreateUserDto(
    string EmployeeId,
    string FullName,
    string Email,
    string Phone,
    Guid RoleId,
    Guid? BranchId
);

public record UpdateUserDto(string FullName, string Phone, Guid RoleId, Guid? BranchId);

public record UserListItemDto(
    Guid Id,
    string EmployeeId,
    string FullName,
    string Email,
    string Phone,
    string Role,
    string? BranchName,
    string Status,
    DateTime CreatedAtUtc
);

public record UserDetailDto(
    Guid Id,
    string EmployeeId,
    string FullName,
    string Email,
    string Phone,
    Guid RoleId,
    string Role,
    Guid? BranchId,
    string? BranchName,
    string Status,
    bool MustChangePassword,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record AssignRoleDto(Guid RoleId);

public record AssignBranchDto(Guid? BranchId);

public record InviteStatusDto(string Status, DateTime? ExpiresAtUtc);
