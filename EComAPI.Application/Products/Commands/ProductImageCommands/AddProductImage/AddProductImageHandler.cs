using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Products.Entities;

namespace EComAPI.Application.Products.Commands.ProductImageCommands.AddProductImage
{
    public class AddProductImageHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public AddProductImageHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            AddProductImageCommand addProductImageCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (addProductImageCommand.ProductId == Guid.Empty)
                    return Result<Guid>.Failure("Product ID is required");

                if (string.IsNullOrWhiteSpace(addProductImageCommand.ImageUrl))
                    return Result<Guid>.Failure("Image URL is required");

                if (addProductImageCommand.DisplayOrder < 0)
                    return Result<Guid>.Failure("Display order cannot be negative");

                var product = await _productRepository.GetProductByIdWithImagesAsync(
                    addProductImageCommand.ProductId,
                    cancellationToken);

                if (product == null)
                    return Result<Guid>.Failure("Product not found");

                var hasActiveImages = product.Images.Any(i => !i.IsDeleted);

                var isPrimary = !hasActiveImages || addProductImageCommand.IsPrimary;

                if (isPrimary && hasActiveImages)
                {
                    foreach (var existingImage in product.Images.Where(i => i.IsPrimary && !i.IsDeleted))
                    {
                        existingImage.UnsetPrimary();
                        await _productRepository.UpdateProductImageAsync(existingImage, cancellationToken);
                    }
                }

                var productImage = new ProductImage(
                    addProductImageCommand.ProductId,
                    addProductImageCommand.ImageUrl,
                    _currentUser.UserId,
                    isPrimary,
                    addProductImageCommand.DisplayOrder
                );

                await _productRepository.AddProductImageAsync(productImage, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(productImage.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to add image: {exception.Message}");
            }
        }
    }
}
