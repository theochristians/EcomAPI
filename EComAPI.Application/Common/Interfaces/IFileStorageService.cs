namespace EComAPI.Application.Common.Interfaces
{
    public sealed record FileObjectInfo(
        long ContentLength,
        string? ContentType,
        string? ETag);

    public interface IFileStorageService
    {
        Task<string> CreateWriteSasUrlAsync(
            string blobPath,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default);

        string GetBlobUrl(string blobPath);

        Task<FileObjectInfo?> GetObjectInfoAsync(
            string blobPath,
            CancellationToken cancellationToken = default);
    }
}
