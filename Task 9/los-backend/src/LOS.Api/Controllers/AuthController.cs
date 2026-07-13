using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LOS.Api.Contracts.Requests;
using LOS.Api.Contracts.Responses;
using LOS.Application.Auth;
using LOS.Application.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LOS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService, IWebHostEnvironment env) : ControllerBase
{
    private bool IsDev => env.IsDevelopment();
    private bool UseSecureCookies => !IsDev || Request.IsHttps;
    private SameSiteMode AuthCookieSameSite => IsDev ? SameSiteMode.Strict : SameSiteMode.None;

    private CookieOptions CreateAuthCookieOptions(string path = "/", DateTime? expires = null)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = UseSecureCookies,
            SameSite = AuthCookieSameSite,
            Path = path,
            IsEssential = true,
        };

        if (expires.HasValue)
        {
            options.Expires = expires.Value;
        }

        return options;
    }

    private static string BuildCrossSiteSetCookieHeader(
        string name,
        string value,
        DateTime? expiresUtc,
        bool delete = false
    )
    {
        var header = $"{name}={value}; path=/; secure; samesite=none; httponly; partitioned";

        if (delete)
        {
            return $"{header}; max-age=0";
        }

        return $"{header}; expires={expiresUtc!.Value.ToString("R", CultureInfo.InvariantCulture)}";
    }

    private void SetAuthCookies(string accessToken, DateTime expiresAt, string refreshToken)
    {
        if (IsDev)
        {
            Response.Cookies.Append(
                "los_access_token",
                accessToken,
                CreateAuthCookieOptions(expires: expiresAt)
            );
            Response.Cookies.Append(
                "los_refresh_token",
                refreshToken,
                CreateAuthCookieOptions(expires: DateTime.UtcNow.AddDays(7))
            );
            return;
        }

        Response.Headers.Append(
            "Set-Cookie",
            BuildCrossSiteSetCookieHeader("los_access_token", accessToken, expiresAt)
        );
        Response.Headers.Append(
            "Set-Cookie",
            BuildCrossSiteSetCookieHeader(
                "los_refresh_token",
                refreshToken,
                DateTime.UtcNow.AddDays(7)
            )
        );
    }

    private void ClearAuthCookies()
    {
        if (IsDev)
        {
            Response.Cookies.Delete("los_access_token", CreateAuthCookieOptions());
            Response.Cookies.Delete("los_refresh_token", CreateAuthCookieOptions());
            return;
        }

        Response.Headers.Append(
            "Set-Cookie",
            BuildCrossSiteSetCookieHeader("los_access_token", string.Empty, null, delete: true)
        );
        Response.Headers.Append(
            "Set-Cookie",
            BuildCrossSiteSetCookieHeader("los_refresh_token", string.Empty, null, delete: true)
        );
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct
    )
    {
        var result = await authService.LoginAsync(
            new LoginRequestDto(request.Email, request.Password),
            ct
        );

        SetAuthCookies(result.AccessToken, result.AccessTokenExpiresAtUtc, result.RefreshToken);

        return Ok(
            new LoginResponse(result.AccessTokenExpiresAtUtc, result.User, result.LandingRoute)
        );
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Refresh(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["los_refresh_token"];

        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("Session expired. Please log in again.");

        var result = await authService.RefreshAsync(refreshToken, ct);

        SetAuthCookies(result.AccessToken, result.AccessTokenExpiresAtUtc, result.RefreshToken);

        return Ok(
            new LoginResponse(result.AccessTokenExpiresAtUtc, result.User, result.LandingRoute)
        );
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["los_refresh_token"];
        if (!string.IsNullOrEmpty(refreshToken))
            await authService.LogoutAsync(refreshToken, ct);

        ClearAuthCookies();
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponseDto>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct
    )
    {
        var result = await authService.ForgotPasswordAsync(request.Email, ct);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponseDto>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct
    )
    {
        var result = await authService.ResetPasswordAsync(
            new ResetPasswordRequestDto(request.Token, request.NewPassword),
            ct
        );
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> Me(CancellationToken ct)
    {
        var userIdClaim =
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        return Ok(await authService.GetProfileAsync(userId, ct));
    }
}
