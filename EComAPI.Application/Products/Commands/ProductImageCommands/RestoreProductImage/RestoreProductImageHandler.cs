using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductImageCommands.RestoreProductImage
{
    public class RestoreProductImageHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreProductImageHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            RestoreProductImageCommand restoreProductImageCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (restoreProductImageCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Image ID is required");

                var productImage = await _productRepository.GetProductImageByIdIncludeDeletedAsync(
                    restoreProductImageCommand.Id,
                    cancellationToken);

                if (productImage == null)
                    return Result<Guid>.Failure("Image not found");

                if (!productImage.IsDeleted)
                    return Result<Guid>.Failure("Image is not deleted");

                // If the image being restored was primary, unset primary from all
                // currently active images first — otherwise there will be two primaries.
                if (productImage.IsPrimary)
                {
                    var product = await _productRepository.GetProductByIdWithImagesAsync(
                        productImage.ProductId,
                        cancellationToken);

                    if (product != null)
                    {
                        foreach (var activeImage in product.Images.Where(i => !i.IsDeleted))
                        {
                            activeImage.UnsetPrimary();
                            await _productRepository.UpdateProductImageAsync(activeImage, cancellationToken);
                        }
                    }
                }

                productImage.Restore();

                await _productRepository.UpdateProductImageAsync(productImage, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(productImage.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to restore image: {exception.Message}");
            }
        }
    }
}

