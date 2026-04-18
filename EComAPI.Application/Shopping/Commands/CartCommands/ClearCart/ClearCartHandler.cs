using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Shopping.Commands.CartCommands.ClearCart
{
    public class ClearCartHandler
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ClearCartHandler(
            ICartRepository cartRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _cartRepository = cartRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            ClearCartCommand clearCartCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                var cart = await _cartRepository.GetCartByUserIdWithItemsAsync(
                    _currentUser.UserId, cancellationToken);

                if (cart == null)
                    return Result<Guid>.Failure("Cart not found");

                cart.Clear(_currentUser.UserId);

                foreach (var cartItem in cart.Items.Where(cartItem => cartItem.DeletedAt != null))
                    await _cartRepository.UpdateCartItemAsync(cartItem, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(cart.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to clear cart: {exception.Message}");
            }
        }
    }
}
