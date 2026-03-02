using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductCommands.RestoreProduct
{
    public class RestoreProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreProductHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            RestoreProductCommand restoreProductCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (restoreProductCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Product ID is required");

                var deletedProductById = await _productRepository.GetProductByIdIncludeDeletedAsync(
                    restoreProductCommand.Id,
                    cancellationToken);

                if (deletedProductById == null)
                    return Result<Guid>.Failure("Product not found");

                if (!deletedProductById.IsDeleted)
                    return Result<Guid>.Failure("Product is not deleted");

                deletedProductById.Restore();

                await _productRepository.UpdateProductAsync(deletedProductById, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(deletedProductById.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to restore product: {exception.Message}");
            }
        }
    }
}
