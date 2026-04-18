using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Shopping.Entities;

namespace EComAPI.Application.Shopping.Commands.WishlistCommands.AddToWishlist
{
    public class AddToWishlistHandler
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public AddToWishlistHandler(
            IWishlistRepository wishlistRepository,
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            AddToWishlistCommand addToWishlistCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (addToWishlistCommand.ProductId == Guid.Empty)
                    return Result<Guid>.Failure("ProductId is required");

                var product = await _productRepository.GetProductByIdAsync(
                    addToWishlistCommand.ProductId, cancellationToken);

                if (product == null)
                    return Result<Guid>.Failure("Product not found");

                if (!product.IsActive)
                    return Result<Guid>.Failure("Product is not active");

                var alreadyExists = await _wishlistRepository.WishlistItemExistsAsync(
                    _currentUser.UserId, addToWishlistCommand.ProductId, cancellationToken);

                if (alreadyExists)
                    return Result<Guid>.Failure("Product already in wishlist");

                var wishlistItem = new Wishlist(
                    _currentUser.UserId,
                    addToWishlistCommand.ProductId,
                    _currentUser.UserId);

                await _wishlistRepository.AddWishlistItemAsync(wishlistItem, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(wishlistItem.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to add to wishlist: {exception.Message}");
            }
        }
    }
}
