using System.Security.Claims;
using BankApi.Application.DTOs.Auth;
using BankApi.Application.Interfaces;
using BankApi.Domain.Constants;
using BankApi.Domain.Entities;
using BankApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Application.Services;

public sealed class AuthService : IAuthService
{
    private const string ObjectIdClaim =
        "http://schemas.microsoft.com/identity/claims/objectidentifier";
    private const string TenantIdClaim = "http://schemas.microsoft.com/identity/claims/tenantid";

    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<AuthExchangeResult> ExchangeEntraPrincipalAsync(
        ClaimsPrincipal entraPrincipal,
        string? ipAddress,
        CancellationToken ct
    )
    {
        var entraObjectId =
            entraPrincipal.FindFirstValue("oid")
            ?? entraPrincipal.FindFirstValue(ObjectIdClaim)
            ?? throw new UnauthorizedAccessException("Missing Entra object id.");

        var tenantId =
            entraPrincipal.FindFirstValue("tid")
            ?? entraPrincipal.FindFirstValue(TenantIdClaim)
            ?? throw new UnauthorizedAccessException("Missing tenant id.");

        var email =
            entraPrincipal.FindFirstValue(ClaimTypes.Upn)
            ?? entraPrincipal.FindFirstValue("preferred_username")
            ?? entraPrincipal.FindFirstValue(ClaimTypes.Email)
            ?? throw new UnauthorizedAccessException("Missing email claim.");

        var displayName =
            entraPrincipal.FindFirstValue("name")
            ?? entraPrincipal.FindFirstValue(ClaimTypes.Name)
            ?? email;

        var user = await _dbContext.AppUsers.FirstOrDefaultAsync(
            x => x.EntraObjectId == entraObjectId && x.TenantId == tenantId,
            ct
        );

        var assignedRole = email.Equals(
            "naikkrish109@gmail.com",
            StringComparison.OrdinalIgnoreCase
        )
            ? AppRoles.Admin
            : AppRoles.Customer;

        if (user is null)
        {
            user = new AppUser
            {
                Id = Guid.NewGuid(),
                EntraObjectId = entraObjectId,
                TenantId = tenantId,
                Email = email,
                DisplayName = displayName,
                Role = assignedRole,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
                LastLoginAtUtc = DateTime.UtcNow,
            };

            _dbContext.AppUsers.Add(user);
        }
        else
        {
            user.Email = email;
            user.DisplayName = displayName;
            user.LastLoginAtUtc = DateTime.UtcNow;
            user.UpdatedAtUtc = DateTime.UtcNow;

            if (email.Equals("naikkrish109@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                user.Role = AppRoles.Admin;
            }
        }

        await _dbContext.SaveChangesAsync(ct);

        var issued = await _tokenService.IssueAsync(user, ipAddress, ct);
        return new AuthExchangeResult(issued.Response, issued.RefreshToken);
    }
}
