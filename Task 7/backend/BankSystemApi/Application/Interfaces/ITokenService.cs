using BankApi.Application.DTOs.Auth;
using BankApi.Domain.Entities;

namespace BankApi.Application.Interfaces;

public interface ITokenService
{
    Task<(AppAuthResponse Response, string RefreshToken)> IssueAsync(
        AppUser user,
        string? ipAddress,
        CancellationToken ct
    );
    Task<(AppAuthResponse Response, string RefreshToken)> RefreshAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken ct
    );
    Task RevokeAsync(string refreshToken, string? ipAddress, CancellationToken ct);
}
