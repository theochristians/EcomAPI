using EComAPI.Application.Common.Commands.Uploads.ConfirmUpload;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Common.Uploads
{
    public class ConfirmUploadHandlerTests
    {
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IFileStorageService> _mockFileStorageService;
        private readonly ConfirmUploadHandler _handler;

        public ConfirmUploadHandlerTests()
        {
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _handler = new ConfirmUploadHandler(_mockCurrentUser.Object, _mockFileStorageService.Object);
        }

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _handler.Handle(new ConfirmUploadCommand(
                "user_avatar",
                "users/abc/avatars/file.jpg"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_PathBukanMilikUser_ShouldReturnFailure()
        {
            var userId = Guid.NewGuid();
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(userId);

            var result = await _handler.Handle(new ConfirmUploadCommand(
                "user_avatar",
                "users/another/avatars/file.jpg"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Blob path does not belong to current user");
        }

        [Fact]
        public async Task Handle_FileTidakAda_ShouldReturnFailure()
        {
            var userId = Guid.NewGuid();
            var blobPath = $"users/{userId:N}/payments/file.jpg";
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
            _mockFileStorageService
                .Setup(x => x.GetObjectInfoAsync(blobPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FileObjectInfo?)null);

            var result = await _handler.Handle(new ConfirmUploadCommand("payment_proof", blobPath));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Uploaded file not found");
        }

        [Fact]
        public async Task Handle_DataValid_ShouldReturnSuccess()
        {
            var userId = Guid.NewGuid();
            var blobPath = $"users/{userId:N}/reviews/file.jpg";
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
            _mockFileStorageService
                .Setup(x => x.GetObjectInfoAsync(blobPath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileObjectInfo(1024, "image/jpeg", "etag-1"));
            _mockFileStorageService
                .Setup(x => x.GetBlobUrl(blobPath))
                .Returns("https://storage.example.com/container/reviews/file.jpg");

            var result = await _handler.Handle(new ConfirmUploadCommand(
                "review_image",
                blobPath,
                ExpectedFileSizeBytes: 1024,
                ExpectedContentType: "image/jpeg"));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.BlobPath.Should().Be(blobPath);
            result.Value.ContentType.Should().Be("image/jpeg");
        }
    }
}
