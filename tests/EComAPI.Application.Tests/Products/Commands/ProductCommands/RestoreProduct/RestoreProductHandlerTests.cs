using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductCommands.RestoreProduct;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Products.Commands.ProductCommands.RestoreProduct
{
    public class RestoreProductHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly RestoreProductHandler _restoreProductHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _categoryId = Guid.NewGuid();

        public RestoreProductHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _restoreProductHandler = new RestoreProductHandler(
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

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var restoreProductResult = await _restoreProductHandler.Handle(new RestoreProductCommand(Guid.NewGuid()));

            restoreProductResult.IsSuccess.Should().BeFalse();
            restoreProductResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var restoreProductResult = await _restoreProductHandler.Handle(new RestoreProductCommand(Guid.Empty));

            restoreProductResult.IsSuccess.Should().BeFalse();
            restoreProductResult.Error.Should().Be("Product ID is required");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockProductRepository.Setup(x => x.GetProductByIdIncludeDeletedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var restoreProductResult = await _restoreProductHandler.Handle(new RestoreProductCommand(Guid.NewGuid()));

            restoreProductResult.IsSuccess.Should().BeFalse();
            restoreProductResult.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_ProductTidakDihapus_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);

            _mockProductRepository.Setup(x => x.GetProductByIdIncludeDeletedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var restoreProductResult = await _restoreProductHandler.Handle(new RestoreProductCommand(product.Id));

            restoreProductResult.IsSuccess.Should().BeFalse();
            restoreProductResult.Error.Should().Be("Product is not deleted");
        }

        [Fact]
        public async Task Handle_ProductDihapus_ShouldRestoreAndReturnID()
        {
            SetupAuthenticatedUser();

            var product = new Product(_categoryId, "Laptop", "laptop", 1000m, _userId);
            product.Delete(_userId);

            _mockProductRepository.Setup(x => x.GetProductByIdIncludeDeletedAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var restoreProductResult = await _restoreProductHandler.Handle(new RestoreProductCommand(product.Id));

            restoreProductResult.IsSuccess.Should().BeTrue();
            restoreProductResult.Value.Should().Be(product.Id);
            product.IsDeleted.Should().BeFalse();
            _mockProductRepository.Verify(x => x.UpdateProductAsync(product, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}