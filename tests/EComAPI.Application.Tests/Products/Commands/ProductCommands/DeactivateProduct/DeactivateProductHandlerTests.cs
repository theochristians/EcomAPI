using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductCommands.DeactivateProduct;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;

namespace EComAPI.Application.Tests.Products.Commands.ProductCommands.DeactivateProduct
{
    public class DeactivateProductHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly DeactivateProductHandler _deactiveProductHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public DeactivateProductHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _deactiveProductHandler = new DeactivateProductHandler(
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

            var deactiveProductResult = await _deactiveProductHandler.Handle(new DeactivateProductCommand(Guid.NewGuid()));

            deactiveProductResult.IsSuccess.Should().BeFalse();
            deactiveProductResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var deactiveProductResult = await _deactiveProductHandler.Handle(new DeactivateProductCommand(Guid.Empty));

            deactiveProductResult.IsSuccess.Should().BeFalse();
            deactiveProductResult.Error.Should().Be("Product ID is required");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var deactiveProductResult = await _deactiveProductHandler.Handle(new DeactivateProductCommand(Guid.NewGuid()));

            deactiveProductResult.IsSuccess.Should().BeFalse();
            deactiveProductResult.Error.Should().Be("Product not found");
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

            var deactiveProductResult = await _deactiveProductHandler.Handle(new DeactivateProductCommand(product.Id));

            deactiveProductResult.IsSuccess.Should().BeFalse();
            deactiveProductResult.Error.Should().Be("Product is deleted and cannot be deactivated");
        }

        [Fact]
        public async Task Handle_ProductSudahInaktif_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var product = new Product(Guid.NewGuid(), "Laptop", "laptop", 1000m, _userId);
            product.Deactivate(_userId);

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var deactiveProductResult = await _deactiveProductHandler.Handle(new DeactivateProductCommand(product.Id));

            deactiveProductResult.IsSuccess.Should().BeFalse();
            deactiveProductResult.Error.Should().Be("Product is already inactive");
        }

        [Fact]
        public async Task Handle_ProductAktif_ShouldDeactivateAndReturnId()
        {
            SetupAuthenticatedUser();

            var product = new Product(Guid.NewGuid(), "Laptop", "laptop", 1000m, _userId);

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var deactiveProductResult = await _deactiveProductHandler.Handle(new DeactivateProductCommand(product.Id));

            deactiveProductResult.IsSuccess.Should().BeTrue();
            deactiveProductResult.Value.Should().Be(product.Id);
            product.IsActive.Should().BeFalse();
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
