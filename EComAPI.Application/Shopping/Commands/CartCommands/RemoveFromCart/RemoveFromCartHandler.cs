using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Shopping.Commands.CartCommands.RemoveFromCart
{
    public class RemoveFromCartHandler
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveFromCartHandler(
            ICartRepository cartRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _cartRepository = cartRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            RemoveFromCartCommand removeFromCartCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (removeFromCartCommand.CartItemId == Guid.Empty)
                    return Result<Guid>.Failure("CartItemId is required");

                var cartItem = await _cartRepository.GetCartItemByIdAsync(
                    removeFromCartCommand.CartItemId, cancellationToken);

                if (cartItem == null)
                    return Result<Guid>.Failure("Cart item not found");

                // Verify cart belongs to current user
                var cart = await _cartRepository.GetCartByUserIdAsync(
                    _currentUser.UserId, cancellationToken);

                if (cart == null || cart.Id != cartItem.CartId)
                    return Result<Guid>.Failure("Cart item not found");

                cartItem.Delete(_currentUser.UserId);
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
                return Result<Guid>.Failure($"Failed to remove from cart: {exception.Message}");
            }
        }
    }
}
