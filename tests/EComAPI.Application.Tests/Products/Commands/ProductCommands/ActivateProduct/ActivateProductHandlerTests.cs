using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductCommands.ActivateProduct;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;

namespace EComAPI.Application.Tests.Products.Commands.ProductCommands.ActivateProduct
{
    public class ActivateProductHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly ActivateProductHandler _activeProductHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public ActivateProductHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _activeProductHandler = new ActivateProductHandler(
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

        private Product CreateInactiveProduct()
        {
            var product = new Product(Guid.NewGuid(), "Laptop", "laptop", 1000m, _userId);
            product.Deactivate(_userId); 
            return product;
        }

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var activeProductResult = await _activeProductHandler.Handle(new ActivateProductCommand(Guid.NewGuid()));

            activeProductResult.IsSuccess.Should().BeFalse();
            activeProductResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var activeProductResult = await _activeProductHandler.Handle(new ActivateProductCommand(Guid.Empty));

            activeProductResult.IsSuccess.Should().BeFalse();
            activeProductResult.Error.Should().Be("Product ID is required");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var activeProductResult = await _activeProductHandler.Handle(new ActivateProductCommand(Guid.NewGuid()));

            activeProductResult.IsSuccess.Should().BeFalse();
            activeProductResult.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_ProductSudahDihapus_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var product = new Product(Guid.NewGuid(), "Laptop", "laptop", 1000m, _userId);
            product.Delete(_userId);

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var activeProductResult = await _activeProductHandler.Handle(new ActivateProductCommand(product.Id));

            activeProductResult.IsSuccess.Should().BeFalse();
            activeProductResult.Error.Should().Be("Deleted product cannot be activated. Restore first");
        }

        [Fact]
        public async Task Handle_ProductSudahAktif_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var product = new Product(Guid.NewGuid(), "Laptop", "laptop", 1000m, _userId);

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var activeProductResult = await _activeProductHandler.Handle(new ActivateProductCommand(product.Id));

            activeProductResult.IsSuccess.Should().BeFalse();
            activeProductResult.Error.Should().Be("Product is already active");
        }

        [Fact]
        public async Task Handle_ProductInaktif_ShouldActivateAndReturnId()
        {
            SetupAuthenticatedUser();

            var product = CreateInactiveProduct();

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var activeProductResult = await _activeProductHandler.Handle(new ActivateProductCommand(product.Id));

            activeProductResult.IsSuccess.Should().BeTrue();
            activeProductResult.Value.Should().Be(product.Id);
            product.IsActive.Should().BeTrue();
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
