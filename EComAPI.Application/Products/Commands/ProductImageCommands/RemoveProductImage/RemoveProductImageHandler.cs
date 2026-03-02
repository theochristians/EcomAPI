using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductImageCommands.RemoveProductImage
{
    public class RemoveProductImageHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveProductImageHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            RemoveProductImageCommand removeProductImageCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (removeProductImageCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Image ID is required");

                var productImage = await _productRepository.GetProductImageByIdAsync(
                    removeProductImageCommand.Id,
                    cancellationToken);

                if (productImage == null)
                    return Result<Guid>.Failure("Image not found");

                if (productImage.IsDeleted)
                    return Result<Guid>.Failure("Image is already deleted");

                var product = await _productRepository.GetProductByIdWithImagesAsync(
                    productImage.ProductId,
                    cancellationToken);

                if (product == null)
                    return Result<Guid>.Failure("Product not found");

                var activeImagesCount = product.Images.Count(i => !i.IsDeleted);

                if (activeImagesCount <= 1)
                    return Result<Guid>.Failure("Cannot remove the last image. Product must have at least one image.");

                if (productImage.IsPrimary)
                {
                    var nextImage = product.Images
                        .Where(i => i.Id != productImage.Id && !i.IsDeleted)
                        .OrderBy(i => i.DisplayOrder)
                        .FirstOrDefault();

                    if (nextImage != null)
                    {
                        nextImage.SetAsPrimary(_currentUser.UserId);
                        await _productRepository.UpdateProductImageAsync(nextImage, cancellationToken);
                    }
                }

                productImage.Delete(_currentUser.UserId);

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
                return Result<Guid>.Failure($"Failed to remove image: {exception.Message}");
            }
        }
    }
}
