namespace EComAPI.Application.Common.Commands.Uploads.ConfirmUpload
{
    public record ConfirmUploadCommand(
        string Purpose,
        string BlobPath,
        long? ExpectedFileSizeBytes = null,
        string? ExpectedContentType = null);
}
