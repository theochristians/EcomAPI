using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Common.Uploads;

namespace EComAPI.Application.Common.Commands.Uploads.GenerateUploadSas
{
    public class GenerateUploadSasHandler
    {
        private readonly ICurrentUser _currentUser;
        private readonly IFileStorageService _fileStorageService;

        public GenerateUploadSasHandler(
            ICurrentUser currentUser,
            IFileStorageService fileStorageService)
        {
            _currentUser = currentUser;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<GenerateUploadSasResult>> Handle(
            GenerateUploadSasCommand generateUploadSasCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<GenerateUploadSasResult>.Failure("User not authenticated");

                if (!UploadPolicies.TryParsePurpose(generateUploadSasCommand.Purpose, out var purpose))
                    return Result<GenerateUploadSasResult>.Failure("Invalid upload purpose");

                if (string.IsNullOrWhiteSpace(generateUploadSasCommand.ContentType))
                    return Result<GenerateUploadSasResult>.Failure("Content type is required");

                var contentType = generateUploadSasCommand.ContentType.Trim().ToLowerInvariant();
                if (!UploadPolicies.IsAllowedContentType(contentType))
                    return Result<GenerateUploadSasResult>.Failure("Unsupported content type");

                if (generateUploadSasCommand.FileSizeBytes <= 0)
                    return Result<GenerateUploadSasResult>.Failure("File size must be greater than 0");

                var maxSizeBytes = UploadPolicies.GetMaxSizeBytes(purpose);
                if (generateUploadSasCommand.FileSizeBytes > maxSizeBytes)
                    return Result<GenerateUploadSasResult>.Failure(
                        $"File is too large. Maximum allowed size is {maxSizeBytes} bytes");

                var extension = UploadPolicies.GetExtension(contentType, generateUploadSasCommand.FileName);
                var uploadId = Guid.NewGuid();
                var blobPath = UploadPathBuilder.BuildPath(
                    purpose,
                    _currentUser.UserId,
                    extension,
                    uploadId);

                var expiresAtUtc = SecurityTime.UtcNow.AddMinutes(10);
                var uploadUrl = await _fileStorageService.CreateWriteSasUrlAsync(
                    blobPath,
                    expiresAtUtc,
                    cancellationToken);

                var blobUrl = _fileStorageService.GetBlobUrl(blobPath);

                return Result<GenerateUploadSasResult>.Success(
                    new GenerateUploadSasResult(
                        uploadId,
                        purpose.ToString(),
                        blobPath,
                        blobUrl,
                        uploadUrl,
                        expiresAtUtc));
            }
            catch (Exception exception)
            {
                return Result<GenerateUploadSasResult>.Failure(
                    $"Failed to generate upload URL: {exception.Message}");
            }
        }
    }
}
