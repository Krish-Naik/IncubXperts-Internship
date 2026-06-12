namespace BankApi.Application.Options;

public sealed class AppJwtOptions
{
    public const string SectionName = "AppJwt";

    public string Issuer { get; set; } = "BankSystemApi";
    public string Audience { get; set; } = "BankSystem";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
    public string RefreshCookieName { get; set; } = "bank_refresh_token";
}
