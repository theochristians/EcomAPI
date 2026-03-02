using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductVariantCommands.UpdateProductVariant
{
    public class UpdateProductVariantHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductVariantHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UpdateProductVariantCommand updateProductVariantCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (updateProductVariantCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Variant ID is required");

                if (updateProductVariantCommand.Stock.HasValue && updateProductVariantCommand.Stock < 0)
                    return Result<Guid>.Failure("Stock cannot be negative");

                var productVariantById = await _productRepository.GetProductVariantByIdAsync(
                    updateProductVariantCommand.Id,
                    cancellationToken);

                if (productVariantById == null)
                    return Result<Guid>.Failure("Variant not found");

                var productWithVariants = await _productRepository.GetProductByIdWithVariantsAsync(
                    productVariantById.ProductId,
                    cancellationToken);

                if (productWithVariants == null)
                    return Result<Guid>.Failure("Product not found");

                var sku = !string.IsNullOrWhiteSpace(updateProductVariantCommand.Sku)
                    ? updateProductVariantCommand.Sku
                    : productVariantById.Sku;

                var stock = updateProductVariantCommand.Stock ?? productVariantById.Stock;

                var priceAdjustment = updateProductVariantCommand.PriceAdjustment ?? productVariantById.PriceAdjustment;

                var size = updateProductVariantCommand.Size != null
                    ? (string.IsNullOrWhiteSpace(updateProductVariantCommand.Size) ? null : updateProductVariantCommand.Size)
                    : productVariantById.Size;

                var color = updateProductVariantCommand.Color != null
                    ? (string.IsNullOrWhiteSpace(updateProductVariantCommand.Color) ? null : updateProductVariantCommand.Color)
                    : productVariantById.Color;

                productVariantById.Update(
                    size,
                    color,
                    priceAdjustment,
                    _currentUser.UserId,
                    sku,
                    stock
                );

                productWithVariants.RecalculateTotalStock();

                await _productRepository.UpdateProductVariantAsync(productVariantById, cancellationToken);
                await _productRepository.UpdateProductAsync(productWithVariants, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(productVariantById.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to update variant: {exception.Message}");
            }
        }
    }
}
