using EComAPI.Domain.Shopping.Entities;

namespace EComAPI.Application.Shopping.Interfaces
{
    public interface IWishlistRepository
    {
        Task<IReadOnlyList<Wishlist>> GetWishlistByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Wishlist?> GetWishlistItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Wishlist?> GetWishlistItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
        Task<bool> WishlistItemExistsAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
        Task AddWishlistItemAsync(Wishlist wishlist, CancellationToken cancellationToken = default);
        Task UpdateWishlistItemAsync(Wishlist wishlist, CancellationToken cancellationToken = default);
    }
}
