namespace LOS.Application.Options;

public class SeedOptions
{
    public const string SectionName = "Seed";
    public string AdminEmail { get; set; } = "admin@los.local";
    public string AdminPassword { get; set; } = "Admin@12345";
}
