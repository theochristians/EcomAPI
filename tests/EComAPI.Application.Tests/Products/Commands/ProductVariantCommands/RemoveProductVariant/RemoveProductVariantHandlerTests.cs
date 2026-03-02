using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductVariantCommands.RemoveProductVariant;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Products.Commands.ProductVariantCommands.RemoveProductVariant
{
    public class RemoveProductVariantHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly RemoveProductVariantHandler _removeProductVariantHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();

        public RemoveProductVariantHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _removeProductVariantHandler = new RemoveProductVariantHandler(
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

            var result = await _removeProductVariantHandler.Handle(new RemoveProductVariantCommand(Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _removeProductVariantHandler.Handle(new RemoveProductVariantCommand(Guid.Empty));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Variant ID is required");
        }

        [Fact]
        public async Task Handle_VariantTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductVariant?)null);

            var result = await _removeProductVariantHandler.Handle(new RemoveProductVariantCommand(Guid.NewGuid()));

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

            var result = await _removeProductVariantHandler.Handle(new RemoveProductVariantCommand(variant.Id));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_VariantTerakhir_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            var variant = new ProductVariant(product.Id, "SKU-001", 10, _userId);
            product.AddVariant(variant);
            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdAsync(variant.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variant);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _removeProductVariantHandler.Handle(new RemoveProductVariantCommand(variant.Id));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Cannot remove the last variant. Product must have at least one variant.");
        }

        [Fact]
        public async Task Handle_Valid_ShouldRemoveVariantAndReturnId()
        {
            SetupAuthenticatedUser();
            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            var variant1 = new ProductVariant(product.Id, "SKU-001", 10, _userId);
            var variant2 = new ProductVariant(product.Id, "SKU-002", 5, _userId);
            product.AddVariant(variant1);
            product.AddVariant(variant2);
            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdAsync(variant1.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variant1);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _removeProductVariantHandler.Handle(new RemoveProductVariantCommand(variant1.Id));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(variant1.Id);
            variant1.IsDeleted.Should().BeTrue();
            _mockProductRepository.Verify(x => x.UpdateProductVariantAsync(variant1, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductRepository.Verify(x => x.UpdateProductAsync(product, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
