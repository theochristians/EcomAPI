using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductVariantCommands.RemoveProductVariant
{
    public class RemoveProductVariantHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveProductVariantHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            RemoveProductVariantCommand removeProductVariantCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (removeProductVariantCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Variant ID is required");

                var productVariantById = await _productRepository.GetProductVariantByIdAsync(
                    removeProductVariantCommand.Id,
                    cancellationToken);

                if (productVariantById == null)
                    return Result<Guid>.Failure("Variant not found");

                var productWithVariants = await _productRepository.GetProductByIdWithVariantsAsync(
                    productVariantById.ProductId,
                    cancellationToken);

                if (productWithVariants == null)
                    return Result<Guid>.Failure("Product not found");

                var activeVariantsCount = productWithVariants.Variants
                    .Count(productVariant => !productVariant.IsDeleted);

                if (activeVariantsCount <= 1)
                    return Result<Guid>.Failure("Cannot remove the last variant. Product must have at least one variant.");

                productVariantById.Delete(_currentUser.UserId);

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
                return Result<Guid>.Failure($"Failed to remove variant: {exception.Message}");
            }
        }
    }
}
