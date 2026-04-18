namespace EComAPI.Application.Common.Commands.Uploads.GenerateUploadSas
{
    public record GenerateUploadSasCommand(
        string Purpose,
        string FileName,
        string ContentType,
        long FileSizeBytes);
}
