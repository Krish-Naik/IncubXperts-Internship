using Microsoft.AspNetCore.Authorization;

namespace BankApi.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission) => Permission = permission;

    public string Permission { get; }
}
