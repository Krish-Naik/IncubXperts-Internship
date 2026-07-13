namespace LOS.Application.Storage;

public record StoredFile(string StorageProvider, string StoragePath);

public interface IFileStorageService
{
    Task<StoredFile> SaveAsync(
        Stream content,
        string fileName,
        string subFolder,
        CancellationToken ct
    );
    Task<Stream> GetAsync(string storagePath, CancellationToken ct);
    Task DeleteAsync(string storagePath, CancellationToken ct);
}

public interface IFileStorageResolver
{
    IFileStorageService Resolve(string storageProvider);
}
