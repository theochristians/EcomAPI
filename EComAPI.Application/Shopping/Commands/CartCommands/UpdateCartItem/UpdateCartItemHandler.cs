using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Shopping.Commands.CartCommands.UpdateCartItem
{
    public class UpdateCartItemHandler
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCartItemHandler(
            ICartRepository cartRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _cartRepository = cartRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UpdateCartItemCommand updateCartItemCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (updateCartItemCommand.CartItemId == Guid.Empty)
                    return Result<Guid>.Failure("CartItemId is required");

                if (updateCartItemCommand.Quantity < 1)
                    return Result<Guid>.Failure("Quantity must be at least 1");

                var cartItem = await _cartRepository.GetCartItemByIdAsync(
                    updateCartItemCommand.CartItemId, cancellationToken);

                if (cartItem == null)
                    return Result<Guid>.Failure("Cart item not found");

                // Verify cart belongs to current user
                var cart = await _cartRepository.GetCartByUserIdAsync(
                    _currentUser.UserId, cancellationToken);

                if (cart == null || cart.Id != cartItem.CartId)
                    return Result<Guid>.Failure("Cart item not found");

                cartItem.UpdateQuantity(updateCartItemCommand.Quantity, _currentUser.UserId);
                await _cartRepository.UpdateCartItemAsync(cartItem, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(cartItem.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to update cart item: {exception.Message}");
            }
        }
    }
}
