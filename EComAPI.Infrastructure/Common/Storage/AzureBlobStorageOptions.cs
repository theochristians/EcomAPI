namespace EComAPI.Infrastructure.Common.Storage
{
    public sealed class AzureBlobStorageOptions
    {
        public const string SectionName = "AzureBlobStorage";

        public bool Enabled { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
    }
}
