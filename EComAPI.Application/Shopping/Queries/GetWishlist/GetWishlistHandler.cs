using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Shopping.DTOs;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Shopping.Queries.GetWishlist
{
    public class GetWishlistHandler
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;

        public GetWishlistHandler(
            IWishlistRepository wishlistRepository,
            IProductRepository productRepository,
            ICurrentUser currentUser)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<WishlistItemDto>>> Handle(
            GetWishlistQuery getWishlistQuery,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<IReadOnlyList<WishlistItemDto>>.Failure("User not authenticated");

                var wishlistItems = await _wishlistRepository.GetWishlistByUserIdAsync(
                    _currentUser.UserId, cancellationToken);

                var wishlistItemDtos = new List<WishlistItemDto>();

                foreach (var wishlistItem in wishlistItems)
                {
                    var product = await _productRepository.GetProductByIdWithImagesAsync(
                        wishlistItem.ProductId, cancellationToken);

                    if (product == null)
                        continue;

                    var primaryImage = product.Images
                        .Where(productImage => productImage.IsPrimary && productImage.DeletedAt == null)
                        .Select(productImage => productImage.ImageUrl)
                        .FirstOrDefault();

                    wishlistItemDtos.Add(new WishlistItemDto(
                        wishlistItem.Id,
                        wishlistItem.ProductId,
                        product.Name,
                        product.Slug,
                        product.BasePrice,
                        primaryImage,
                        product.IsActive,
                        wishlistItem.CreatedAt
                    ));
                }

                return Result<IReadOnlyList<WishlistItemDto>>.Success(wishlistItemDtos);
            }
            catch (DomainException domainException)
            {
                return Result<IReadOnlyList<WishlistItemDto>>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<IReadOnlyList<WishlistItemDto>>.Failure($"Failed to get wishlist: {exception.Message}");
            }
        }
    }
}
