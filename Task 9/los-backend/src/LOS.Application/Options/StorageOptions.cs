namespace LOS.Application.Options;

public class StorageOptions
{
    public const string SectionName = "Storage";
    public string Provider { get; set; } = "LocalDisk";
    public string LocalDiskBasePath { get; set; } = "App_Data/uploads";
    public SupabaseStorageSettings Supabase { get; set; } = new();
}

public class SupabaseStorageSettings
{
    public string Url { get; set; } = string.Empty;
    public string ServiceRoleKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = "kyc-documents";
}
