namespace EComAPI.Application.Common.Commands.Uploads.GenerateUploadSas
{
    public record GenerateUploadSasResult(
        Guid UploadId,
        string Purpose,
        string BlobPath,
        string BlobUrl,
        string UploadUrl,
        DateTime ExpiresAtUtc);
}
