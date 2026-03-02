using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductVariantCommands.RestoreProductVariant;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Products.Commands.ProductVariantCommands.RestoreProductVariant
{
    public class RestoreProductVariantHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly RestoreProductVariantHandler _restoreProductVariantHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();

        public RestoreProductVariantHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _restoreProductVariantHandler = new RestoreProductVariantHandler(
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

        private ProductVariant BuildDeletedVariant(Guid productId)
        {
            var variant = new ProductVariant(productId, "SKU-001", 10, _userId);
            variant.Delete(_userId);
            return variant;
        }

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _restoreProductVariantHandler.Handle(new RestoreProductVariantCommand(Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _restoreProductVariantHandler.Handle(new RestoreProductVariantCommand(Guid.Empty));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Variant ID is required");
        }

        [Fact]
        public async Task Handle_VariantTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdIncludeDeletedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductVariant?)null);

            var result = await _restoreProductVariantHandler.Handle(new RestoreProductVariantCommand(Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Variant not found");
        }

        [Fact]
        public async Task Handle_VariantBelumDihapus_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var variant = new ProductVariant(Guid.NewGuid(), "SKU-001", 10, _userId);
            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdIncludeDeletedAsync(variant.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variant);

            var result = await _restoreProductVariantHandler.Handle(new RestoreProductVariantCommand(variant.Id));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Variant is not deleted");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var variant = BuildDeletedVariant(Guid.NewGuid());
            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdIncludeDeletedAsync(variant.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variant);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithVariantsAsync(variant.ProductId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _restoreProductVariantHandler.Handle(new RestoreProductVariantCommand(variant.Id));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_Valid_ShouldRestoreVariantAndReturnId()
        {
            SetupAuthenticatedUser();
            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            var variant = BuildDeletedVariant(product.Id);
            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdIncludeDeletedAsync(variant.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variant);
            _mockProductRepository
                .Setup(x => x.GetProductByIdWithVariantsAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _restoreProductVariantHandler.Handle(new RestoreProductVariantCommand(variant.Id));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(variant.Id);
            variant.IsDeleted.Should().BeFalse();
            _mockProductRepository.Verify(x => x.UpdateProductVariantAsync(variant, It.IsAny<CancellationToken>()), Times.Once);
            _mockProductRepository.Verify(x => x.UpdateProductAsync(product, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
