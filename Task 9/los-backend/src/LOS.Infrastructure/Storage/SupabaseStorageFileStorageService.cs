using System.Net.Http.Headers;
using LOS.Application.Options;
using LOS.Application.Storage;
using Microsoft.Extensions.Options;

namespace LOS.Infrastructure.Storage;

public class SupabaseStorageFileStorageService(
    IHttpClientFactory httpClientFactory,
    IOptions<StorageOptions> options
) : IFileStorageService
{
    private readonly SupabaseStorageSettings _settings = options.Value.Supabase;

    public async Task<StoredFile> SaveAsync(
        Stream content,
        string fileName,
        string subFolder,
        CancellationToken ct
    )
    {
        var safeName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var objectPath = $"{subFolder}/{safeName}".Replace('\\', '/').TrimStart('/');

        using var client = CreateClient();
        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(fileName));

        using var response = await client.PostAsync(
            $"object/{_settings.Bucket}/{objectPath}",
            streamContent,
            ct
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Failed to upload file to Supabase Storage ({(int)response.StatusCode}): {error}"
            );
        }

        return new StoredFile("Supabase", objectPath);
    }

    public async Task<Stream> GetAsync(string storagePath, CancellationToken ct)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"object/{_settings.Bucket}/{storagePath}", ct);

        if (!response.IsSuccessStatusCode)
            throw new FileNotFoundException("Document not found.", storagePath);

        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        return new MemoryStream(bytes);
    }

    public async Task DeleteAsync(string storagePath, CancellationToken ct)
    {
        using var client = CreateClient();
        using var response = await client.DeleteAsync(
            $"object/{_settings.Bucket}/{storagePath}",
            ct
        );
        if (
            !response.IsSuccessStatusCode
            && response.StatusCode != System.Net.HttpStatusCode.NotFound
        )
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(
                $"Failed to delete file from Supabase Storage ({(int)response.StatusCode}): {error}"
            );
        }
    }

    private HttpClient CreateClient()
    {
        if (
            string.IsNullOrWhiteSpace(_settings.Url)
            || string.IsNullOrWhiteSpace(_settings.ServiceRoleKey)
        )
        {
            throw new InvalidOperationException(
                "Storage:Supabase:Url and Storage:Supabase:ServiceRoleKey must be configured to use the Supabase storage provider."
            );
        }

        var client = httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_settings.Url.TrimEnd('/') + "/storage/v1/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _settings.ServiceRoleKey
        );
        client.DefaultRequestHeaders.Add("apikey", _settings.ServiceRoleKey);
        return client;
    }

    private static string GetContentType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream",
        };
}
