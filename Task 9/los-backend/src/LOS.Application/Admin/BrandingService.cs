using LOS.Application.Storage;
using LOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LOS.Application.Admin;

public class BrandingService(LOSDbContext db, IFileStorageService storage)
{
    public async Task<BrandingSettings> UpdateLogoAsync(
        Stream content,
        string fileName,
        long sizeBytes,
        CancellationToken ct
    )
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext != ".png" && ext != ".svg")
            throw new InvalidOperationException("Only PNG or SVG logos are allowed.");
        if (sizeBytes > 1 * 1024 * 1024)
            throw new InvalidOperationException("Logo must be under 1MB.");

        var stored = await storage.SaveAsync(content, fileName, "branding", ct);
        var settings = await db.Set<BrandingSettings>().FirstOrDefaultAsync(ct);
        if (settings is null)
        {
            settings = new BrandingSettings { Id = Guid.NewGuid() };
            db.Add(settings);
        }
        settings.LogoStoragePath = stored.StoragePath;
        settings.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return settings;
    }
}
