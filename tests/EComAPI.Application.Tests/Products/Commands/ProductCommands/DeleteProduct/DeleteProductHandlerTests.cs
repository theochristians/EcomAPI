using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Commands.ProductCommands.DeleteProduct;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;

namespace EComAPI.Application.Tests.Products.Commands.DeleteProduct
{
    public class DeleteProductHandlerTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly DeleteProductHandler _deleteProductHandler ;

        private readonly Guid _userId = Guid.NewGuid();

        public DeleteProductHandlerTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _deleteProductHandler = new DeleteProductHandler(
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

            var deleteProductResult = await _deleteProductHandler.Handle(new DeleteProductCommand(Guid.NewGuid()));

            deleteProductResult.IsSuccess.Should().BeFalse();
            deleteProductResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_IdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var deleteProductResult = await _deleteProductHandler.Handle(new DeleteProductCommand(Guid.Empty));

            deleteProductResult.IsSuccess.Should().BeFalse();
            deleteProductResult.Error.Should().Be("Product ID is required");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockProductRepository
                .Setup(x => x.GetProductByIdWithAllDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

            var deleteProductResult = await _deleteProductHandler.Handle(new DeleteProductCommand(Guid.NewGuid()));

            deleteProductResult.IsSuccess.Should().BeFalse();
            deleteProductResult.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_ProductSudahDihapus_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var product = new Product(Guid.NewGuid(), "Laptop", "laptop", 1000m, _userId);
            product.Delete(_userId);

            _mockProductRepository
                .Setup(x => x.GetProductByIdWithAllDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var deleteProductResult = await _deleteProductHandler.Handle(new DeleteProductCommand(product.Id));

            deleteProductResult.IsSuccess.Should().BeFalse();
            deleteProductResult.Error.Should().Be("Product is already deleted");
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldDeleteAndReturnId()
        {
            SetupAuthenticatedUser();

            var product = new Product(Guid.NewGuid(), "Laptop", "laptop", 1000m, _userId);

            _mockProductRepository
                .Setup(x => x.GetProductByIdWithAllDetailsAsync(product.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            var deleteProductResult = await _deleteProductHandler.Handle(new DeleteProductCommand(product.Id));

            deleteProductResult.IsSuccess.Should().BeTrue();
            deleteProductResult.Value.Should().Be(product.Id);
            product.IsDeleted.Should().BeTrue();
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
