using System.Security.Claims;
using BankApi.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace BankApi.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement
    )
    {
        var permissions = context
            .User.FindAll(AppClaimTypes.Permission)
            .Select(x => x.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
