using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LOS.Application.Options;
using LOS.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LOS.Infrastructure.Identity;

public class JwtTokenGenerator(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public (string AccessToken, DateTime ExpiresAtUtc) CreateAccessToken(InternalUser user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.Name),
            new("employee_id", user.EmployeeId),
        };

        if (user.BranchId.HasValue)
        {
            claims.Add(new Claim("branch_id", user.BranchId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
