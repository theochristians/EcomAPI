namespace EComAPI.API.Common.Uploads.Dtos
{
    public record ConfirmUploadRequest(
        string Purpose,
        string BlobPath,
        long? ExpectedFileSizeBytes = null,
        string? ExpectedContentType = null);
}
