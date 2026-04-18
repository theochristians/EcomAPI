using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Shopping.Commands.WishlistCommands.AddToWishlist;
using EComAPI.Application.Shopping.Interfaces;

namespace EComAPI.Application.Tests.Shopping.Commands.AddToWishlist
{
    public class AddToWishlistHandlerTests
    {
        private readonly Mock<IWishlistRepository> _mockWishlistRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly AddToWishlistHandler _addToWishlistHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _productId = Guid.NewGuid();

        public AddToWishlistHandlerTests()
        {
            _mockWishlistRepository = new Mock<IWishlistRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _addToWishlistHandler = new AddToWishlistHandler(
                _mockWishlistRepository.Object,
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

            var result = await _addToWishlistHandler.Handle(new AddToWishlistCommand(_productId));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_ProductIdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _addToWishlistHandler.Handle(new AddToWishlistCommand(Guid.Empty));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("ProductId is required");
        }

        [Fact]
        public async Task Handle_ProductTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(_productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EComAPI.Domain.Products.Entities.Product?)null);

            var result = await _addToWishlistHandler.Handle(new AddToWishlistCommand(_productId));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product not found");
        }

        [Fact]
        public async Task Handle_ProdukSudahDiWishlist_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var product = new EComAPI.Domain.Products.Entities.Product(
                Guid.NewGuid(), "Test Product", "test-product", 50000, _userId);

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(_productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _mockWishlistRepository
                .Setup(x => x.WishlistItemExistsAsync(_userId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _addToWishlistHandler.Handle(new AddToWishlistCommand(_productId));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product already in wishlist");
        }

        [Fact]
        public async Task Handle_ValidInput_ShouldReturnWishlistItemId()
        {
            SetupAuthenticatedUser();

            var product = new EComAPI.Domain.Products.Entities.Product(
                Guid.NewGuid(), "Test Product", "test-product", 50000, _userId);

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(_productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            _mockWishlistRepository
                .Setup(x => x.WishlistItemExistsAsync(_userId, _productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _addToWishlistHandler.Handle(new AddToWishlistCommand(_productId));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
        }
    }
}
