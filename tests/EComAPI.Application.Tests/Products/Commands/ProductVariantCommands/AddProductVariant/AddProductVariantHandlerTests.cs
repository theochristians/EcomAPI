using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductVariantCommands.AddProductVariant;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;

namespace EComAPI.Application.Tests.Products.Commands.AddProductVariant
{
    public class AddProductVariantHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly AddProductVariantHandler _handler;

        private readonly Guid _userId = Guid.NewGuid();

        public AddProductVariantHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _handler = new AddProductVariantHandler(
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

            var result = await _handler.Handle(new AddProductVariantCommand(Guid.NewGuid(), "SKU-001", 10));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_ProductIdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _handler.Handle(new AddProductVariantCommand(Guid.Empty, "SKU-001", 10));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product ID is required");
        }

        [Fact]
        public async Task Handle_SkuKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _handler.Handle(new AddProductVariantCommand(Guid.NewGuid(), "", 10));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("SKU is required");
        }

        [Fact]
        public async Task Handle_StokNegatif_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _handler.Handle(new AddProductVariantCommand(Guid.NewGuid(), "SKU-001", -1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Stock cannot be negative");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockProductRepository
                .Setup(x => x.GetProductByIdWithVariantsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _handler.Handle(new AddProductVariantCommand(Guid.NewGuid(), "SKU-001", 10));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_SkuDuplikat_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var product = CreateProduct();

            // Add an existing variant with same SKU
            var existingVariant = new ProductVariant(product.Id, "SKU-001", 5, _userId);
            product.AddVariant(existingVariant);

            _mockProductRepository
                .Setup(x => x.GetProductByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _handler.Handle(new AddProductVariantCommand(product.Id, "SKU-001", 10));

            // Domain throws DomainException for duplicate SKU
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("SKU-001");
        }

        [Fact]
        public async Task Handle_ValidInput_ShouldAddVariantAndReturnId()
        {
            SetupAuthenticatedUser();

            var product = CreateProduct();

            _mockProductRepository
                .Setup(x => x.GetProductByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _handler.Handle(new AddProductVariantCommand(product.Id, "SKU-001", 10, 0m, "L", "Red"));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();

            _mockProductRepository.Verify(
                x => x.AddProductVariantAsync(It.IsAny<ProductVariant>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
