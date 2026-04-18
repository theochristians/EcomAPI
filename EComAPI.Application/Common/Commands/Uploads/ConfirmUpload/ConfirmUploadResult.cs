namespace EComAPI.Application.Common.Commands.Uploads.ConfirmUpload
{
    public record ConfirmUploadResult(
        string Purpose,
        string BlobPath,
        string BlobUrl,
        long ContentLength,
        string? ContentType,
        string? ETag);
}
