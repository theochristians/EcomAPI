using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductImageCommands.RemoveProductImage;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Products.Commands.ProductImageCommands.RemoveProductImage
{
    public class RemoveProductImageHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly RemoveProductImageHandler _removeProductImageHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();

        public RemoveProductImageHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _removeProductImageHandler = new RemoveProductImageHandler(
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

        private ProductImage BuildActiveImage(Guid productId) =>
            new ProductImage(productId, "https://img.example.com/1.jpg", _userId, false, 1);

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _removeProductImageHandler.Handle(new RemoveProductImageCommand(Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _removeProductImageHandler.Handle(new RemoveProductImageCommand(Guid.Empty));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Image ID is required");
        }

        [Fact]
        public async Task Handle_ImageTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductImage?)null);

            var result = await _removeProductImageHandler.Handle(new RemoveProductImageCommand(Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Image not found");
        }

        [Fact]
        public async Task Handle_ImageSudahDihapus_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var image = BuildActiveImage(Guid.NewGuid());
            image.Delete(_userId);
            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(image.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image);

            var result = await _removeProductImageHandler.Handle(new RemoveProductImageCommand(image.Id));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Image is already deleted");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var productId = Guid.NewGuid();
            var image = BuildActiveImage(productId);
            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(image.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _removeProductImageHandler.Handle(new RemoveProductImageCommand(image.Id));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_GambarTerakhir_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            var image = new ProductImage(product.Id, "https://img.example.com/1.jpg", _userId, true, 1);
            product.AddImage(image);
            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(image.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _removeProductImageHandler.Handle(new RemoveProductImageCommand(image.Id));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Cannot remove the last image. Product must have at least one image.");
        }

        [Fact]
        public async Task Handle_Valid_ShouldRemoveImageAndReturnId()
        {
            SetupAuthenticatedUser();
            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            var image1 = new ProductImage(product.Id, "https://img.example.com/1.jpg", _userId, false, 1);
            var image2 = new ProductImage(product.Id, "https://img.example.com/2.jpg", _userId, false, 2);
            product.AddImage(image1);
            product.AddImage(image2);
            _mockProductRepository
                .Setup(x => x.GetProductImageByIdAsync(image1.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(image1);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _removeProductImageHandler.Handle(new RemoveProductImageCommand(image1.Id));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(image1.Id);
            image1.IsDeleted.Should().BeTrue();
            _mockProductRepository.Verify(x => x.UpdateProductImageAsync(image1, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
