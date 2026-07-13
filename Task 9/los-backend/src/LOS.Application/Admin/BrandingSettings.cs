namespace LOS.Application.Admin;

public class BrandingSettings
{
    public Guid Id { get; set; }
    public string LogoStoragePath { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = "#0d6efd";
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
