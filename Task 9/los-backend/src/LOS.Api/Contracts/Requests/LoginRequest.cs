namespace LOS.Api.Contracts.Requests;

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Token, string NewPassword);
