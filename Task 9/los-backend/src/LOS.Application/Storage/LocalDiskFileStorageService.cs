using LOS.Application.Options;
using LOS.Application.Storage;
using Microsoft.Extensions.Options;

namespace LOS.Infrastructure.Storage;

public class LocalDiskFileStorageService(IOptions<StorageOptions> options) : IFileStorageService
{
    private readonly string _basePath = options.Value.LocalDiskBasePath;

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string fileName,
        string subFolder,
        CancellationToken ct
    )
    {
        var folder = Path.Combine(_basePath, subFolder);
        Directory.CreateDirectory(folder);
        var safeName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(folder, safeName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, ct);

        return new StoredFile("LocalDisk", Path.Combine(subFolder, safeName));
    }

    public Task<Stream> GetAsync(string storagePath, CancellationToken ct)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Document not found.", storagePath);

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
