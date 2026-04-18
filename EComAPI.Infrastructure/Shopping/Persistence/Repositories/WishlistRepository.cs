using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Shopping.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Shopping.Persistence.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly AppDbContext _appDbContext;

        public WishlistRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IReadOnlyList<Wishlist>> GetWishlistByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Wishlists
                .Where(wishlist => wishlist.UserId == userId)
                .OrderByDescending(wishlist => wishlist.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Wishlist?> GetWishlistItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Wishlists
                .FirstOrDefaultAsync(wishlist => wishlist.Id == id, cancellationToken);
        }

        public async Task<Wishlist?> GetWishlistItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Wishlists
                .FirstOrDefaultAsync(wishlist => wishlist.UserId == userId && wishlist.ProductId == productId, cancellationToken);
        }

        public async Task<bool> WishlistItemExistsAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Wishlists
                .AnyAsync(wishlist => wishlist.UserId == userId && wishlist.ProductId == productId, cancellationToken);
        }

        public async Task AddWishlistItemAsync(Wishlist wishlist, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Wishlists.AddAsync(wishlist, cancellationToken);
        }

        public Task UpdateWishlistItemAsync(Wishlist wishlist, CancellationToken cancellationToken = default)
        {
            _appDbContext.Wishlists.Update(wishlist);
            return Task.CompletedTask;
        }
    }
}
