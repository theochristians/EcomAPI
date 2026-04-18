namespace EComAPI.API.Common.Uploads.Dtos
{
    public record GenerateUploadSasRequest(
        string Purpose,
        string FileName,
        string ContentType,
        long FileSizeBytes);
}
