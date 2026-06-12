using System.Security.Claims;
using BankApi.Application.DTOs.Auth;

namespace BankApi.Application.Interfaces;

public interface IAuthService
{
    Task<AuthExchangeResult> ExchangeEntraPrincipalAsync(
        ClaimsPrincipal entraPrincipal,
        string? ipAddress,
        CancellationToken ct
    );
}
