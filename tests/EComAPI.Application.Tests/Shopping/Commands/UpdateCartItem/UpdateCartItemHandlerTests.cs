using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Shopping.Commands.CartCommands.UpdateCartItem;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Shopping.Entities;

namespace EComAPI.Application.Tests.Shopping.Commands.UpdateCartItem
{
    public class UpdateCartItemHandlerTests
    {
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UpdateCartItemHandler _updateCartItemHandler;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _cartItemId = Guid.NewGuid();

        public UpdateCartItemHandlerTests()
        {
            _mockCartRepository = new Mock<ICartRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _updateCartItemHandler = new UpdateCartItemHandler(
                _mockCartRepository.Object,
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

            var result = await _updateCartItemHandler.Handle(new UpdateCartItemCommand(_cartItemId, 2));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_CartItemIdKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _updateCartItemHandler.Handle(new UpdateCartItemCommand(Guid.Empty, 2));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("CartItemId is required");
        }

        [Fact]
        public async Task Handle_QuantityKurangDariSatu_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _updateCartItemHandler.Handle(new UpdateCartItemCommand(_cartItemId, 0));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Quantity must be at least 1");
        }

        [Fact]
        public async Task Handle_CartItemTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockCartRepository
                .Setup(x => x.GetCartItemByIdAsync(_cartItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CartItem?)null);

            var result = await _updateCartItemHandler.Handle(new UpdateCartItemCommand(_cartItemId, 2));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Cart item not found");
        }

        [Fact]
        public async Task Handle_ValidInput_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();

            var cart = new Cart(_userId, _userId);
            var cartItem = new CartItem(cart.Id, Guid.NewGuid(), 1, _userId);

            _mockCartRepository
                .Setup(x => x.GetCartItemByIdAsync(_cartItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cartItem);

            _mockCartRepository
                .Setup(x => x.GetCartByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cart);

            var result = await _updateCartItemHandler.Handle(new UpdateCartItemCommand(_cartItemId, 3));

            result.IsSuccess.Should().BeTrue();
        }
    }
}
