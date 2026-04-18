using EComAPI.API.Common;
using EComAPI.API.Common.Uploads.Dtos;
using EComAPI.Application.Common.Commands.Uploads.ConfirmUpload;
using EComAPI.Application.Common.Commands.Uploads.GenerateUploadSas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EComAPI.API.Common.Uploads.Controllers
{
    [ApiController]
    [Route("api/uploads")]
    [Produces("application/json")]
    [Authorize]
    public class UploadsController : BaseController
    {
        private readonly GenerateUploadSasHandler _generateUploadSasHandler;
        private readonly ConfirmUploadHandler _confirmUploadHandler;

        public UploadsController(
            GenerateUploadSasHandler generateUploadSasHandler,
            ConfirmUploadHandler confirmUploadHandler)
        {
            _generateUploadSasHandler = generateUploadSasHandler;
            _confirmUploadHandler = confirmUploadHandler;
        }

        /// <summary>
        /// Membuat SAS upload URL agar frontend upload file langsung ke Azure Blob Storage.
        /// </summary>
        [HttpPost("sas")]
        [EnableRateLimiting("upload-sas")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateUploadSas(
            [FromBody] GenerateUploadSasRequest generateUploadSasRequest,
            CancellationToken cancellationToken)
        {
            var command = new GenerateUploadSasCommand(
                generateUploadSasRequest.Purpose,
                generateUploadSasRequest.FileName,
                generateUploadSasRequest.ContentType,
                generateUploadSasRequest.FileSizeBytes);

            var result = await _generateUploadSasHandler.Handle(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequestResponse(result.Error ?? "Failed to generate upload URL");

            return SuccessResponse(
                new
                {
                    uploadId = result.Value!.UploadId,
                    purpose = result.Value.Purpose,
                    blobPath = result.Value.BlobPath,
                    blobUrl = result.Value.BlobUrl,
                    uploadUrl = result.Value.UploadUrl,
                    expiresAtUtc = result.Value.ExpiresAtUtc,
                    requiredHeaders = new Dictionary<string, string>
                    {
                        ["x-ms-blob-type"] = "BlockBlob",
                        ["Content-Type"] = generateUploadSasRequest.ContentType
                    }
                },
                "Upload URL generated");
        }

        /// <summary>
        /// Konfirmasi bahwa file sudah ter-upload ke blob storage.
        /// </summary>
        [HttpPost("confirm")]
        [EnableRateLimiting("upload-confirm")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConfirmUpload(
            [FromBody] ConfirmUploadRequest confirmUploadRequest,
            CancellationToken cancellationToken)
        {
            var command = new ConfirmUploadCommand(
                confirmUploadRequest.Purpose,
                confirmUploadRequest.BlobPath,
                confirmUploadRequest.ExpectedFileSizeBytes,
                confirmUploadRequest.ExpectedContentType);

            var result = await _confirmUploadHandler.Handle(command, cancellationToken);
            if (!result.IsSuccess)
                return BadRequestResponse(result.Error ?? "Failed to confirm upload");

            return SuccessResponse(
                new
                {
                    purpose = result.Value!.Purpose,
                    blobPath = result.Value.BlobPath,
                    blobUrl = result.Value.BlobUrl,
                    contentLength = result.Value.ContentLength,
                    contentType = result.Value.ContentType,
                    eTag = result.Value.ETag
                },
                "Upload confirmed");
        }
    }
}
