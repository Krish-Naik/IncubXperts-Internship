using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace LOS.Infrastructure.Identity;

public class PasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(new object(), password);

    public bool Verify(string password, string hash) =>
        _hasher.VerifyHashedPassword(new object(), hash, password)
        != PasswordVerificationResult.Failed;
}

public static class TokenGenerator
{
    public static string CreateSecureToken() =>
        Convert
            .ToBase64String(RandomNumberGenerator.GetBytes(48))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
}
