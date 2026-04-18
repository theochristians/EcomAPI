using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Shopping.DTOs;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Shopping.Queries.GetCart
{
    public class GetCartHandler
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;

        public GetCartHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ICurrentUser currentUser)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<CartDto>> Handle(
            GetCartQuery getCartQuery,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<CartDto>.Failure("User not authenticated");

                var cart = await _cartRepository.GetCartByUserIdWithItemsAsync(
                    _currentUser.UserId, cancellationToken);

                if (cart == null)
                    return Result<CartDto>.Failure("Cart not found");

                var cartItemDtos = new List<CartItemDto>();

                foreach (var cartItem in cart.Items.Where(cartItem => cartItem.DeletedAt == null))
                {
                    var variant = await _productRepository.GetProductVariantWithProductAsync(
                        cartItem.ProductVariantId, cancellationToken);

                    if (variant?.Product == null)
                        continue;

                    var unitPrice = variant.Product.BasePrice + (variant.PriceAdjustment ?? 0);

                    cartItemDtos.Add(new CartItemDto(
                        cartItem.Id,
                        cartItem.ProductVariantId,
                        variant.Product.Name,
                        variant.Sku,
                        variant.Size,
                        variant.Color,
                        unitPrice,
                        cartItem.Quantity,
                        unitPrice * cartItem.Quantity
                    ));
                }

                var totalPrice = cartItemDtos.Sum(i => i.Subtotal);
                var totalItems = cartItemDtos.Sum(i => i.Quantity);

                var cartDto = new CartDto(
                    cart.Id,
                    cart.UserId,
                    cartItemDtos,
                    totalPrice,
                    totalItems
                );

                return Result<CartDto>.Success(cartDto);
            }
            catch (DomainException domainException)
            {
                return Result<CartDto>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<CartDto>.Failure($"Failed to get cart: {exception.Message}");
            }
        }
    }
}
