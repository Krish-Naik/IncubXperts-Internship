namespace LOS.Application.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "LOS.Api";
    public string Audience { get; set; } = "LOS.Frontend";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
