using EComAPI.Domain.Shopping.Entities;

namespace EComAPI.Application.Shopping.Interfaces
{
    public interface ICartRepository
    {
        // CART
        Task<Cart?> GetCartByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Cart?> GetCartByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddCartAsync(Cart cart, CancellationToken cancellationToken = default);
        Task UpdateCartAsync(Cart cart, CancellationToken cancellationToken = default);

        // CART ITEM
        Task<CartItem?> GetCartItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddCartItemAsync(CartItem cartItem, CancellationToken cancellationToken = default);
        Task UpdateCartItemAsync(CartItem cartItem, CancellationToken cancellationToken = default);
        Task<bool> CartItemExistsAsync(Guid cartId, Guid productVariantId, CancellationToken cancellationToken = default);
    }
}
