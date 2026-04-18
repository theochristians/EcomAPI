using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Common.Uploads;

namespace EComAPI.Application.Common.Commands.Uploads.ConfirmUpload
{
    public class ConfirmUploadHandler
    {
        private readonly ICurrentUser _currentUser;
        private readonly IFileStorageService _fileStorageService;

        public ConfirmUploadHandler(
            ICurrentUser currentUser,
            IFileStorageService fileStorageService)
        {
            _currentUser = currentUser;
            _fileStorageService = fileStorageService;
        }

        public async Task<Result<ConfirmUploadResult>> Handle(
            ConfirmUploadCommand confirmUploadCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<ConfirmUploadResult>.Failure("User not authenticated");

                if (!UploadPolicies.TryParsePurpose(confirmUploadCommand.Purpose, out var purpose))
                    return Result<ConfirmUploadResult>.Failure("Invalid upload purpose");

                if (string.IsNullOrWhiteSpace(confirmUploadCommand.BlobPath))
                    return Result<ConfirmUploadResult>.Failure("Blob path is required");

                var blobPath = confirmUploadCommand.BlobPath.Trim();
                if (!UploadPathBuilder.IsPathOwnedByUser(purpose, _currentUser.UserId, blobPath))
                    return Result<ConfirmUploadResult>.Failure("Blob path does not belong to current user");

                var objectInfo = await _fileStorageService.GetObjectInfoAsync(blobPath, cancellationToken);
                if (objectInfo is null)
                    return Result<ConfirmUploadResult>.Failure("Uploaded file not found");

                if (confirmUploadCommand.ExpectedFileSizeBytes.HasValue &&
                    confirmUploadCommand.ExpectedFileSizeBytes.Value != objectInfo.ContentLength)
                {
                    return Result<ConfirmUploadResult>.Failure("Uploaded file size mismatch");
                }

                if (!string.IsNullOrWhiteSpace(confirmUploadCommand.ExpectedContentType) &&
                    !string.Equals(
                        confirmUploadCommand.ExpectedContentType.Trim(),
                        objectInfo.ContentType,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return Result<ConfirmUploadResult>.Failure("Uploaded content type mismatch");
                }

                var blobUrl = _fileStorageService.GetBlobUrl(blobPath);
                return Result<ConfirmUploadResult>.Success(
                    new ConfirmUploadResult(
                        purpose.ToString(),
                        blobPath,
                        blobUrl,
                        objectInfo.ContentLength,
                        objectInfo.ContentType,
                        objectInfo.ETag));
            }
            catch (Exception exception)
            {
                return Result<ConfirmUploadResult>.Failure(
                    $"Failed to confirm upload: {exception.Message}");
            }
        }
    }
}
