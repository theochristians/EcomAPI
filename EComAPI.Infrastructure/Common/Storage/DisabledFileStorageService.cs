using EComAPI.Application.Common.Interfaces;

namespace EComAPI.Infrastructure.Common.Storage
{
    public class DisabledFileStorageService : IFileStorageService
    {
        public Task<string> CreateWriteSasUrlAsync(
            string blobPath,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Azure Blob storage is not configured.");
        }

        public string GetBlobUrl(string blobPath)
        {
            throw new InvalidOperationException("Azure Blob storage is not configured.");
        }

        public Task<FileObjectInfo?> GetObjectInfoAsync(
            string blobPath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Azure Blob storage is not configured.");
        }
    }
}
