namespace BankApi.Application.DTOs.Auth;

public sealed record UserInfoResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    IReadOnlyList<string> Permissions
);

public sealed record AppAuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    UserInfoResponse User
);

public sealed record AuthExchangeResult(AppAuthResponse Response, string RefreshToken);
