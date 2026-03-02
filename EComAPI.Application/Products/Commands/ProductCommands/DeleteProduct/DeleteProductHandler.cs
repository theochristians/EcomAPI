using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductCommands.DeleteProduct
{
    public class DeleteProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            DeleteProductCommand deleteProductCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (deleteProductCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Product ID is required");

                var productById = await _productRepository.GetProductByIdWithAllDetailsAsync(
                    deleteProductCommand.Id,
                    cancellationToken);

                if (productById == null)
                    return Result<Guid>.Failure("Product not found");

                if (productById.IsDeleted)
                    return Result<Guid>.Failure("Product is already deleted");

                productById.Delete(_currentUser.UserId);

                await _productRepository.UpdateProductAsync(productById, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(productById.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to delete product: {exception.Message}");
            }
        }
    }
}
