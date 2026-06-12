using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BankApi.Application.DTOs.Auth;
using BankApi.Application.Interfaces;
using BankApi.Application.Options;
using BankApi.Domain.Constants;
using BankApi.Domain.Entities;
using BankApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BankApi.Application.Services;

public sealed class TokenService : ITokenService
{
    private readonly AppDbContext _dbContext;
    private readonly AppJwtOptions _jwtOptions;

    public TokenService(AppDbContext dbContext, IOptions<AppJwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<(AppAuthResponse Response, string RefreshToken)> IssueAsync(
        AppUser user,
        string? ipAddress,
        CancellationToken ct
    )
    {
        var permissions = BankApi.Domain.Constants.AppPermissions.GetByRole(user.Role);
        var accessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role),
            new(AppClaimTypes.InternalUserId, user.Id.ToString()),
            new(AppClaimTypes.EntraObjectId, user.EntraObjectId),
            new(AppClaimTypes.TenantId, user.TenantId),
            new(AppClaimTypes.DisplayName, user.DisplayName),
        };

        claims.AddRange(permissions.Select(p => new Claim(AppClaimTypes.Permission, p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: accessTokenExpiresAtUtc,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        var rawRefreshToken = GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            AppUserId = user.Id,
            TokenHash = Hash(rawRefreshToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedByIp = ipAddress,
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(ct);

        var response = new AppAuthResponse(
            accessToken,
            accessTokenExpiresAtUtc,
            new UserInfoResponse(user.Id, user.Email, user.DisplayName, user.Role, permissions)
        );

        return (response, rawRefreshToken);
    }

    public async Task<(AppAuthResponse Response, string RefreshToken)> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken ct
    )
    {
        var tokenHash = Hash(refreshToken);

        var existing = await _dbContext
            .RefreshTokens.Include(x => x.AppUser)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (existing is null || !existing.IsActive || !existing.AppUser.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.RevokedByIp = ipAddress;

        var issued = await IssueAsync(existing.AppUser, ipAddress, ct);
        existing.ReplacedByTokenHash = Hash(issued.RefreshToken);

        await _dbContext.SaveChangesAsync(ct);
        return issued;
    }

    public async Task RevokeAsync(string refreshToken, string? ipAddress, CancellationToken ct)
    {
        var tokenHash = Hash(refreshToken);

        var existing = await _dbContext.RefreshTokens.FirstOrDefaultAsync(
            x => x.TokenHash == tokenHash,
            ct
        );

        if (existing is null || existing.IsRevoked)
            return;

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.RevokedByIp = ipAddress;

        await _dbContext.SaveChangesAsync(ct);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }
}
