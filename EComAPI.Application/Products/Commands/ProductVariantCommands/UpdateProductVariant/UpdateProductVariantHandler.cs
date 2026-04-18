using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Products.Commands.ProductVariantCommands.UpdateProductVariant
{
    public class UpdateProductVariantHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockLogRepository _stockLogRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductVariantHandler(
            IProductRepository productRepository,
            IStockLogRepository stockLogRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _stockLogRepository = stockLogRepository;
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

                var stockBefore = productVariantById.Stock;

                productVariantById.Update(
                    size,
                    color,
                    priceAdjustment,
                    _currentUser.UserId,
                    sku,
                    stock
                );

                // Log stock change if stock was modified
                if (productVariantById.Stock != stockBefore)
                {
                    var quantityChange = productVariantById.Stock - stockBefore;
                    var stockLog = new StockLog(
                        productVariantId: productVariantById.Id,
                        type: "adjustment",
                        quantityChange: quantityChange,
                        stockBefore: stockBefore,
                        stockAfter: productVariantById.Stock,
                        createdBy: _currentUser.UserId,
                        note: $"Stock adjusted via variant update from {stockBefore} to {productVariantById.Stock}");

                    await _stockLogRepository.AddLogAsync(stockLog, cancellationToken);
                }

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
