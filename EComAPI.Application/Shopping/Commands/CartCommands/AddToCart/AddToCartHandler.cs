using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Shopping.Entities;

namespace EComAPI.Application.Shopping.Commands.CartCommands.AddToCart
{
    public class AddToCartHandler
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public AddToCartHandler(
            ICartRepository cartRepository,
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            AddToCartCommand addToCartCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (addToCartCommand.ProductVariantId == Guid.Empty)
                    return Result<Guid>.Failure("ProductVariantId is required");

                if (addToCartCommand.Quantity < 1)
                    return Result<Guid>.Failure("Quantity must be at least 1");

                var variant = await _productRepository.GetProductVariantByIdAsync(
                    addToCartCommand.ProductVariantId, cancellationToken);

                if (variant == null)
                    return Result<Guid>.Failure("Product variant not found");

                if (!variant.IsActive)
                    return Result<Guid>.Failure("Product variant is not active");

                if (variant.Stock < addToCartCommand.Quantity)
                    return Result<Guid>.Failure($"Insufficient stock. Available: {variant.Stock}");

                var cart = await _cartRepository.GetCartByUserIdWithItemsAsync(
                    _currentUser.UserId, cancellationToken);

                if (cart == null)
                    return Result<Guid>.Failure("Cart not found");

                var existingItem = cart.Items.FirstOrDefault(
                    cartItem => cartItem.ProductVariantId == addToCartCommand.ProductVariantId && cartItem.DeletedAt == null);

                if (existingItem != null)
                {
                    existingItem.UpdateQuantity(existingItem.Quantity + addToCartCommand.Quantity, _currentUser.UserId);
                    await _cartRepository.UpdateCartItemAsync(existingItem, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return Result<Guid>.Success(existingItem.Id);
                }

                var cartItem = new CartItem(
                    cart.Id,
                    addToCartCommand.ProductVariantId,
                    addToCartCommand.Quantity,
                    _currentUser.UserId);

                await _cartRepository.AddCartItemAsync(cartItem, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(cartItem.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to add to cart: {exception.Message}");
            }
        }
    }
}
