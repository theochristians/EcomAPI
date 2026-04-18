using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Products.Entities;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Products.Commands.ProductVariantCommands.AddProductVariant
{
    public class AddProductVariantHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockLogRepository _stockLogRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public AddProductVariantHandler(
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
            AddProductVariantCommand addProductVariantCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (addProductVariantCommand.ProductId == Guid.Empty)
                    return Result<Guid>.Failure("Product ID is required");

                if (string.IsNullOrWhiteSpace(addProductVariantCommand.Sku))
                    return Result<Guid>.Failure("SKU is required");

                if (addProductVariantCommand.Stock < 0)
                    return Result<Guid>.Failure("Stock cannot be negative");

                var productWithVariants = await _productRepository.GetProductByIdWithVariantsAsync(
                    addProductVariantCommand.ProductId,
                    cancellationToken);

                if (productWithVariants == null)
                    return Result<Guid>.Failure("Product not found");

                var productVariant = new ProductVariant(
                    addProductVariantCommand.ProductId,
                    addProductVariantCommand.Sku,
                    addProductVariantCommand.Stock,
                    _currentUser.UserId,
                    addProductVariantCommand.PriceAdjustment,
                    addProductVariantCommand.Size,
                    addProductVariantCommand.Color
                );

                productWithVariants.AddVariant(productVariant);

                await _productRepository.AddProductVariantAsync(productVariant, cancellationToken);

                // Log initial stock
                if (productVariant.Stock > 0)
                {
                    var stockLog = new StockLog(
                        productVariantId: productVariant.Id,
                        type: "initial",
                        quantityChange: productVariant.Stock,
                        stockBefore: 0,
                        stockAfter: productVariant.Stock,
                        createdBy: _currentUser.UserId,
                        note: "Initial stock for new variant");

                    await _stockLogRepository.AddLogAsync(stockLog, cancellationToken);
                }

                await _productRepository.UpdateProductAsync(productWithVariants, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(productVariant.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to add variant: {exception.Message}");
            }
        }
    }
}
