using LOS.Application.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace LOS.Infrastructure.Storage;

public class FileStorageResolver(IServiceProvider serviceProvider) : IFileStorageResolver
{
    public IFileStorageService Resolve(string storageProvider) =>
        storageProvider switch
        {
            "Supabase" => serviceProvider.GetRequiredService<SupabaseStorageFileStorageService>(),
            _ => serviceProvider.GetRequiredService<LocalDiskFileStorageService>(),
        };
}
