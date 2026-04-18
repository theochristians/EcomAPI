using EComAPI.Application.Common.Commands.Uploads.GenerateUploadSas;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Common.Uploads
{
    public class GenerateUploadSasHandlerTests
    {
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IFileStorageService> _mockFileStorageService;
        private readonly GenerateUploadSasHandler _handler;

        public GenerateUploadSasHandlerTests()
        {
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockFileStorageService = new Mock<IFileStorageService>();
            _handler = new GenerateUploadSasHandler(_mockCurrentUser.Object, _mockFileStorageService.Object);
        }

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _handler.Handle(new GenerateUploadSasCommand(
                "user_avatar",
                "avatar.jpg",
                "image/jpeg",
                1024));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_ContentTypeTidakDidukung_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var result = await _handler.Handle(new GenerateUploadSasCommand(
                "user_avatar",
                "avatar.gif",
                "image/gif",
                1024));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Unsupported content type");
        }

        [Fact]
        public async Task Handle_FileAvatarTerlaluBesar_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(Guid.NewGuid());

            var result = await _handler.Handle(new GenerateUploadSasCommand(
                "user_avatar",
                "avatar.jpg",
                "image/jpeg",
                3L * 1024L * 1024L));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("File is too large");
        }

        [Fact]
        public async Task Handle_DataValid_ShouldReturnUploadTicket()
        {
            var userId = Guid.NewGuid();
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(userId);
            _mockFileStorageService
                .Setup(x => x.CreateWriteSasUrlAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://storage.example.com/upload-sas");
            _mockFileStorageService
                .Setup(x => x.GetBlobUrl(It.IsAny<string>()))
                .Returns<string>(path => $"https://storage.example.com/container/{path}");

            var result = await _handler.Handle(new GenerateUploadSasCommand(
                "product_image",
                "product.png",
                "image/png",
                1024));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Purpose.Should().Be("ProductImage");
            result.Value.BlobPath.Should().Contain($"users/{userId:N}/products/");
            result.Value.UploadUrl.Should().Contain("upload-sas");
        }
    }
}
