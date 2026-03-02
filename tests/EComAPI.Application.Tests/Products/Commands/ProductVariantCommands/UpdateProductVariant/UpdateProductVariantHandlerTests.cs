using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductVariantCommands.UpdateProductVariant;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Products.Commands.ProductVariantCommands.UpdateProductVariant
{
    public class UpdateProductVariantHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UpdateProductVariantHandler _updateProductVariantHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();

        public UpdateProductVariantHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _updateProductVariantHandler = new UpdateProductVariantHandler(
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

        private ProductVariant BuildActiveVariant(Guid productId) =>
            new ProductVariant(productId, "SKU-001", 10, _userId);

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _updateProductVariantHandler.Handle(new UpdateProductVariantCommand(Guid.NewGuid(), null, null, null, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _updateProductVariantHandler.Handle(new UpdateProductVariantCommand(Guid.Empty, null, null, null, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Variant ID is required");
        }

        [Fact]
        public async Task Handle_StokNegatif_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _updateProductVariantHandler.Handle(new UpdateProductVariantCommand(Guid.NewGuid(), null, null, null, null, -1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Stock cannot be negative");
        }

        [Fact]
        public async Task Handle_VariantTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductVariant?)null);

            var result = await _updateProductVariantHandler.Handle(new UpdateProductVariantCommand(Guid.NewGuid(), null, null, null, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Variant not found");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var variant = BuildActiveVariant(Guid.NewGuid());
            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdAsync(variant.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variant);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithVariantsAsync(variant.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _updateProductVariantHandler.Handle(new UpdateProductVariantCommand(variant.Id, null, null, null, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_Valid_ShouldUpdateVariantAndReturnId()
        {
            SetupAuthenticatedUser();
            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            var variant = BuildActiveVariant(product.Id);
            product.AddVariant(variant);
            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdAsync(variant.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variant);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _updateProductVariantHandler.Handle(
                new UpdateProductVariantCommand(variant.Id, "M", "Black", 50m, "SKU-UPD", 20));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(variant.Id);
            variant.Size.Should().Be("M");
            variant.Color.Should().Be("Black");
            variant.PriceAdjustment.Should().Be(50m);
            variant.Stock.Should().Be(20);
            _mockProductRepository.Verify(x => x.UpdateProductVariantAsync(variant, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductRepository.Verify(x => x.UpdateProductAsync(product, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
