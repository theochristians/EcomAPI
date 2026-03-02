using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductCommands.UpdateProduct;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EcomAPI.Application.Tests.Products.Commands.ProductCommands.UpdateProduct
{
    public class UpdateProductHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UpdateProductHandler _updateProductHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();

        public UpdateProductHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _updateProductHandler = new UpdateProductHandler(
                _mockProductRepository.Object,
                _mockCategoryRepository.Object,
                _mockCurrentUser.Object,
                _mockUnitOfWork.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private Product BuildActiveProduct() =>
            new Product(_categoryId, "Laptop", "laptop", 1500m, _userId);

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _updateProductHandler.Handle(new UpdateProductCommand(Guid.NewGuid(), null, null, null, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _updateProductHandler.Handle(new UpdateProductCommand(Guid.Empty, null, null, null, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product ID is required");
        }

        [Fact]
        public async Task Handle_HargaNegatif_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _updateProductHandler.Handle(new UpdateProductCommand(Guid.NewGuid(), null, null, -1m, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Base price cannot be negative");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var result = await _updateProductHandler.Handle(new UpdateProductCommand(Guid.NewGuid(), null, null, null, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_SlugBaruSudahDipakai_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var product = BuildActiveProduct();
            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            _mockProductRepository
                .Setup(x => x.ProductExistsBySlugAsync("existing-slug", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _updateProductHandler.Handle(new UpdateProductCommand(product.Id, null, "existing-slug", null, null, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product slug already exists");
        }

        [Fact]
        public async Task Handle_CategoryTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var newCategoryId = Guid.NewGuid();
            var product = BuildActiveProduct();
            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);
            _mockCategoryRepository
                .Setup(x => x.CategoryExistsAsync(newCategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _updateProductHandler.Handle(new UpdateProductCommand(product.Id, null, null, null, newCategoryId, null));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Category not found");
        }

        [Fact]
        public async Task Handle_Valid_ShouldUpdateProductAndReturnId()
        {
            SetupAuthenticatedUser();
            var product = BuildActiveProduct();
            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var result = await _updateProductHandler.Handle(
                new UpdateProductCommand(product.Id, "Updated Laptop", "updated-laptop", 1200m, null, "Updated description"));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(product.Id);
            product.Name.Should().Be("Updated Laptop");
            product.Slug.Should().Be("updated-laptop");
            product.Description.Should().Be("Updated description");
            product.BasePrice.Should().Be(1200m);
            _mockProductRepository.Verify(x => x.UpdateProductAsync(product, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
