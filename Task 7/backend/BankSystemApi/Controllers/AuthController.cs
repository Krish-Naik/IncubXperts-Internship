using BankApi.Application.DTOs.Auth;
using BankApi.Application.DTOs.Responses;
using BankApi.Application.Interfaces;
using BankApi.Application.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BankApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly AppJwtOptions _jwtOptions;

    public AuthController(
        IAuthService authService,
        ITokenService tokenService,
        IOptions<AppJwtOptions> jwtOptions
    )
    {
        _authService = authService;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    [Authorize(AuthenticationSchemes = "EntraId")]
    [HttpPost("exchange")]
    public async Task<ActionResult<ApiResponse<AppAuthResponse>>> Exchange(CancellationToken ct)
    {
        var result = await _authService.ExchangeEntraPrincipalAsync(
            User,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            ct
        );

        SetRefreshCookie(result.RefreshToken);

        return Ok(ApiResponse<AppAuthResponse>.Ok(result.Response, "Token exchange successful"));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AppAuthResponse>>> Refresh(CancellationToken ct)
    {
        if (
            !Request.Cookies.TryGetValue(_jwtOptions.RefreshCookieName, out var refreshToken)
            || string.IsNullOrWhiteSpace(refreshToken)
        )
        {
            return Unauthorized(ApiResponse<AppAuthResponse>.Fail("Refresh token missing."));
        }

        var result = await _tokenService.RefreshAsync(
            refreshToken,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            ct
        );

        SetRefreshCookie(result.RefreshToken);

        return Ok(ApiResponse<AppAuthResponse>.Ok(result.Response, "Token refreshed successfully"));
    }

    [Authorize(AuthenticationSchemes = "AppJwt")]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout(CancellationToken ct)
    {
        if (
            Request.Cookies.TryGetValue(_jwtOptions.RefreshCookieName, out var refreshToken)
            && !string.IsNullOrWhiteSpace(refreshToken)
        )
        {
            await _tokenService.RevokeAsync(
                refreshToken,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                ct
            );
        }

        var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var isDevelopment = env.IsDevelopment();

        Response.Cookies.Delete(
            _jwtOptions.RefreshCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDevelopment,
                SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
                IsEssential = true,
            }
        );

        return Ok(ApiResponse<object>.Ok(null, "Logged out successfully"));
    }

    [Authorize(AuthenticationSchemes = "AppJwt")]
    [HttpGet("me")]
    public ActionResult<ApiResponse<object>> Me()
    {
        var permissions = User
            .Claims.Where(c => c.Type == BankApi.Domain.Constants.AppClaimTypes.Permission)
            .Select(c => c.Value)
            .ToList();

        var result = new
        {
            UserId = User.FindFirst("bank:user_id")?.Value,
            EntraObjectId = User.FindFirst("bank:entra_oid")?.Value,
            TenantId = User.FindFirst("bank:tenant_id")?.Value,
            DisplayName = User.FindFirst("bank:display_name")?.Value,
            Email = User.FindFirst(
                System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email
            )?.Value,
            Name = User.FindFirst(
                System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name
            )?.Value,
            Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
            Permissions = permissions,
        };

        return Ok(ApiResponse<object>.Ok(result, "Current user fetched successfully"));
    }

    private void SetRefreshCookie(string refreshToken)
    {
        var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var isDevelopment = env.IsDevelopment();

        Response.Cookies.Append(
            _jwtOptions.RefreshCookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDevelopment,
                SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
                IsEssential = true,
            }
        );
    }
}
