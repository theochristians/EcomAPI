using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductVariantCommands.RestoreProductVariant
{
    public class RestoreProductVariantHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreProductVariantHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            RestoreProductVariantCommand restoreProductVariantCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (restoreProductVariantCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Variant ID is required");

                var deletedProductVariantById = await _productRepository.GetProductVariantByIdIncludeDeletedAsync(
                    restoreProductVariantCommand.Id,
                    cancellationToken);

                if (deletedProductVariantById == null)
                    return Result<Guid>.Failure("Variant not found");

                if (!deletedProductVariantById.IsDeleted)
                    return Result<Guid>.Failure("Variant is not deleted");

                var productWithVariants = await _productRepository.GetProductByIdWithVariantsAsync(
                    deletedProductVariantById.ProductId,
                    cancellationToken);

                if (productWithVariants == null)
                    return Result<Guid>.Failure("Product not found");

                deletedProductVariantById.Restore();

                productWithVariants.RecalculateTotalStock();

                await _productRepository.UpdateProductVariantAsync(deletedProductVariantById, cancellationToken);
                await _productRepository.UpdateProductAsync(productWithVariants, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(deletedProductVariantById.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to restore variant: {exception.Message}");
            }
        }
    }
}

