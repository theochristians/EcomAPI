using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Shopping.Commands.WishlistCommands.RemoveFromWishlist
{
    public class RemoveFromWishlistHandler
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveFromWishlistHandler(
            IWishlistRepository wishlistRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _wishlistRepository = wishlistRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            RemoveFromWishlistCommand removeFromWishlistCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (removeFromWishlistCommand.WishlistItemId == Guid.Empty)
                    return Result<Guid>.Failure("WishlistItemId is required");

                var wishlistItem = await _wishlistRepository.GetWishlistItemByIdAsync(
                    removeFromWishlistCommand.WishlistItemId, cancellationToken);

                if (wishlistItem == null)
                    return Result<Guid>.Failure("Wishlist item not found");

                if (wishlistItem.UserId != _currentUser.UserId)
                    return Result<Guid>.Failure("Wishlist item not found");

                wishlistItem.Delete(_currentUser.UserId);
                await _wishlistRepository.UpdateWishlistItemAsync(wishlistItem, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(wishlistItem.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to remove from wishlist: {exception.Message}");
            }
        }
    }
}
