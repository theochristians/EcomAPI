using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductImageCommands.RestoreProductImage;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Products.Commands.ProductImageCommands.RestoreProductImage
{
    public class RestoreProductImageHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly RestoreProductImageHandler _restoreProductImageHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public RestoreProductImageHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _restoreProductImageHandler = new RestoreProductImageHandler(
                _mockProductRepository.Object,
                _mockCurrentUser.Object,
                _mockUnitOfWork.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private ProductImage BuildDeletedImage(Guid productId)
        {
            var image = new ProductImage(productId, "https://img.example.com/1.jpg", _userId, false, 1);
            image.Delete(_userId);
            return image;
        }

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _restoreProductImageHandler.Handle(new RestoreProductImageCommand(Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _restoreProductImageHandler.Handle(new RestoreProductImageCommand(Guid.Empty));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Image ID is required");
        }

        [Fact]
        public async Task Handle_ImageTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockProductRepository
                .Setup(x => x.GetProductImageByIdIncludeDeletedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductImage?)null);

            var result = await _restoreProductImageHandler.Handle(new RestoreProductImageCommand(Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Image not found");
        }

        [Fact]
        public async Task Handle_ImageBelumDihapus_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var image = new ProductImage(Guid.NewGuid(), "https://img.example.com/1.jpg", _userId, false, 1);
            _mockProductRepository
                .Setup(x => x.GetProductImageByIdIncludeDeletedAsync(image.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image);

            var result = await _restoreProductImageHandler.Handle(new RestoreProductImageCommand(image.Id));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Image is not deleted");
        }

        [Fact]
        public async Task Handle_Valid_ShouldRestoreImageAndReturnId()
        {
            SetupAuthenticatedUser();
            var image = BuildDeletedImage(Guid.NewGuid());
            _mockProductRepository
                .Setup(x => x.GetProductImageByIdIncludeDeletedAsync(image.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image);

            var result = await _restoreProductImageHandler.Handle(new RestoreProductImageCommand(image.Id));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(image.Id);
            image.IsDeleted.Should().BeFalse();
            _mockProductRepository.Verify(x => x.UpdateProductImageAsync(image, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
