using LOS.Application.Auth.DTOs;
using LOS.Application.Users.DTOs;

namespace LOS.Application.Auth.DTOs;

public record LoginRequestDto(string Email, string Password);

public record LoginResponseDto(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    UserProfileDto User,
    string LandingRoute
);

public record RefreshTokenRequestDto(string RefreshToken);

public record ForgotPasswordRequestDto(string Email);

public record ResetPasswordRequestDto(string Token, string NewPassword);

public record MessageResponseDto(string Message);

public record UserProfileDto(
    Guid Id,
    string EmployeeId,
    string FullName,
    string Email,
    string Phone,
    string Role,
    Guid? BranchId,
    string? BranchName,
    string Status,
    bool MustChangePassword
);
