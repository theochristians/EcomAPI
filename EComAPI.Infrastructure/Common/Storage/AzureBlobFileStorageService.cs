using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using EComAPI.Application.Common.Interfaces;

namespace EComAPI.Infrastructure.Common.Storage
{
    public class AzureBlobFileStorageService : IFileStorageService
    {
        private readonly AzureBlobStorageOptions _options;
        private readonly BlobServiceClient _serviceClient;
        private readonly BlobContainerClient _containerClient;

        public AzureBlobFileStorageService(AzureBlobStorageOptions options)
        {
            _options = options;

            var tokenCredential = BuildCredential(options);
            var serviceUri = new Uri($"https://{options.AccountName}.blob.core.windows.net");
            _serviceClient = new BlobServiceClient(serviceUri, tokenCredential);
            _containerClient = _serviceClient.GetBlobContainerClient(options.ContainerName);
        }

        public async Task<string> CreateWriteSasUrlAsync(
            string blobPath,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default)
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);
            var expiresAt = new DateTimeOffset(expiresAtUtc, TimeSpan.Zero);
            var startsOn = SecurityTime.UtcNowOffset.AddMinutes(-2);
            var delegationKey = await _serviceClient
                .GetUserDelegationKeyAsync(startsOn, expiresAt, cancellationToken);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _options.ContainerName,
                BlobName = blobPath,
                Resource = "b",
                StartsOn = startsOn,
                ExpiresOn = expiresAt
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

            var sasToken = sasBuilder
                .ToSasQueryParameters(delegationKey, _options.AccountName)
                .ToString();

            return $"{blobClient.Uri}?{sasToken}";
        }

        public string GetBlobUrl(string blobPath)
            => _containerClient.GetBlobClient(blobPath).Uri.ToString();

        public async Task<FileObjectInfo?> GetObjectInfoAsync(
            string blobPath,
            CancellationToken cancellationToken = default)
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);
            var existsResponse = await blobClient.ExistsAsync(cancellationToken);
            if (!existsResponse.Value)
                return null;

            var propertiesResponse = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
            var properties = propertiesResponse.Value;
            return new FileObjectInfo(
                properties.ContentLength,
                properties.ContentType,
                properties.ETag.ToString());
        }

        private static TokenCredential BuildCredential(AzureBlobStorageOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.TenantId) &&
                !string.IsNullOrWhiteSpace(options.ClientId) &&
                !string.IsNullOrWhiteSpace(options.ClientSecret))
            {
                return new ClientSecretCredential(
                    options.TenantId,
                    options.ClientId,
                    options.ClientSecret);
            }

            return new DefaultAzureCredential();
        }
    }
}
