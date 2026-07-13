namespace LOS.Api.Contracts.Requests;

public record CreateUserRequest(
    string EmployeeId,
    string FullName,
    string Email,
    string Phone,
    Guid RoleId,
    Guid? BranchId
);

public record UpdateUserRequest(string FullName, string Phone, Guid RoleId, Guid? BranchId);

public record AssignRoleRequest(Guid RoleId);

public record AssignBranchRequest(Guid? BranchId);
