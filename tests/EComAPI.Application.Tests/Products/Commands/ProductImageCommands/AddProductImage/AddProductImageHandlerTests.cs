using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductImageCommands.AddProductImage;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;

namespace EComAPI.Application.Tests.Products.Commands.AddProductImage
{
    public class AddProductImageHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly AddProductImageHandler _handler;

        private readonly Guid _userId = Guid.NewGuid();

        public AddProductImageHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _handler = new AddProductImageHandler(
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

        private Product CreateProduct() =>
            new Product(Guid.NewGuid(), "Laptop", "laptop", 1000m, _userId);

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _handler.Handle(new AddProductImageCommand(Guid.NewGuid(), "https://img.example.com/1.jpg", true, 1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_ProductIdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _handler.Handle(new AddProductImageCommand(Guid.Empty, "https://img.example.com/1.jpg", true, 1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product ID is required");
        }

        [Fact]
        public async Task Handle_ImageUrlKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _handler.Handle(new AddProductImageCommand(Guid.NewGuid(), "", true, 1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Image URL is required");
        }

        [Fact]
        public async Task Handle_DisplayOrderNegatif_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _handler.Handle(new AddProductImageCommand(Guid.NewGuid(), "https://img.example.com/1.jpg", true, -1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Display order cannot be negative");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _handler.Handle(new AddProductImageCommand(Guid.NewGuid(), "https://img.example.com/1.jpg", true, 1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_GambarPertama_HarusJadiPrimary()
        {
            SetupAuthenticatedUser();

            var product = CreateProduct(); // No images yet

            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _handler.Handle(new AddProductImageCommand(product.Id, "https://img.example.com/1.jpg", false, 1));

            result.IsSuccess.Should().BeTrue();
            _mockProductRepository.Verify(
                x => x.AddProductImageAsync(It.Is<ProductImage>(img => img.IsPrimary), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidInput_ShouldReturnImageId()
        {
            SetupAuthenticatedUser();

            var product = CreateProduct();

            _mockProductRepository
                .Setup(x => x.GetProductByIdWithImagesAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _handler.Handle(new AddProductImageCommand(product.Id, "https://img.example.com/1.jpg", true, 1));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
        }
    }
}
