using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Shopping.Commands.CartCommands.AddToCart;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Shopping.Entities;

namespace EComAPI.Application.Tests.Shopping.Commands.AddToCart
{
    public class AddToCartHandlerTests
    {
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly AddToCartHandler _addToCartHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _variantId = Guid.NewGuid();
        private readonly Guid _cartId = Guid.NewGuid();

        public AddToCartHandlerTests()
        {
            _mockCartRepository = new Mock<ICartRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _addToCartHandler = new AddToCartHandler(
                _mockCartRepository.Object,
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

            var result = await _addToCartHandler.Handle(new AddToCartCommand(_variantId, 1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_ProductVariantIdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _addToCartHandler.Handle(new AddToCartCommand(Guid.Empty, 1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("ProductVariantId is required");
        }

        [Fact]
        public async Task Handle_QuantityKurangDariSatu_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _addToCartHandler.Handle(new AddToCartCommand(_variantId, 0));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Quantity must be at least 1");
        }

        [Fact]
        public async Task Handle_VariantTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdAsync(_variantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EComAPI.Domain.Products.Entities.ProductVariant?)null);

            var result = await _addToCartHandler.Handle(new AddToCartCommand(_variantId, 1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Product variant not found");
        }

        [Fact]
        public async Task Handle_CartTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var variant = new EComAPI.Domain.Products.Entities.ProductVariant(
                Guid.NewGuid(), "SKU-001", 50, _userId);

            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdAsync(_variantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variant);

            _mockCartRepository
                .Setup(x => x.GetCartByUserIdWithItemsAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Cart?)null);

            var result = await _addToCartHandler.Handle(new AddToCartCommand(_variantId, 1));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Cart not found");
        }

        [Fact]
        public async Task Handle_ItemBaruDitambahkan_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();

            var variant = new EComAPI.Domain.Products.Entities.ProductVariant(
                Guid.NewGuid(), "SKU-001", 50, _userId);

            _mockProductRepository
                .Setup(x => x.GetProductVariantByIdAsync(_variantId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(variant);

            var cart = new Cart(_userId, _userId);

            _mockCartRepository
                .Setup(x => x.GetCartByUserIdWithItemsAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var result = await _addToCartHandler.Handle(new AddToCartCommand(_variantId, 2));

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
        }
    }
}
