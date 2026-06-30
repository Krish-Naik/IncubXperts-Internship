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

    private void SetAuthCookies(string accessToken, DateTime expiresAt, string refreshToken)
    {
        var accessTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !IsDev,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = expiresAt,
            IsEssential = true,
        };

        var refreshTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !IsDev,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = DateTime.UtcNow.AddDays(7),
            IsEssential = true,
        };

        Response.Cookies.Append("los_access_token", accessToken, accessTokenOptions);
        Response.Cookies.Append("los_refresh_token", refreshToken, refreshTokenOptions);
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("los_access_token");
        Response.Cookies.Delete("los_refresh_token", new CookieOptions { Path = "/api/auth" });
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
