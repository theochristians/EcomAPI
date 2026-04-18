using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Shopping.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Shopping.Persistence.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _appDbContext;

        public CartRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Cart?> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Carts
                .FirstOrDefaultAsync(cart => cart.UserId == userId, cancellationToken);
        }

        public async Task<Cart?> GetCartByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Carts
                .Include(cart => cart.Items)
                .FirstOrDefaultAsync(cart => cart.UserId == userId, cancellationToken);
        }

        public async Task AddCartAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Carts.AddAsync(cart, cancellationToken);
        }

        public Task UpdateCartAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            _appDbContext.Carts.Update(cart);
            return Task.CompletedTask;
        }

        public async Task<CartItem?> GetCartItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.CartItems
                .FirstOrDefaultAsync(cartItem => cartItem.Id == id, cancellationToken);
        }

        public async Task AddCartItemAsync(CartItem cartItem, CancellationToken cancellationToken = default)
        {
            await _appDbContext.CartItems.AddAsync(cartItem, cancellationToken);
        }

        public Task UpdateCartItemAsync(CartItem cartItem, CancellationToken cancellationToken = default)
        {
            _appDbContext.CartItems.Update(cartItem);
            return Task.CompletedTask;
        }

        public async Task<bool> CartItemExistsAsync(Guid cartId, Guid productVariantId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.CartItems
                .AnyAsync(cartItem => cartItem.CartId == cartId && cartItem.ProductVariantId == productVariantId, cancellationToken);
        }
    }
}
