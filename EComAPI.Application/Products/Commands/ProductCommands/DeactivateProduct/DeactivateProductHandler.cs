using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductCommands.DeactivateProduct
{
    public class DeactivateProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateProductHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            DeactivateProductCommand deactivateProductCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (deactivateProductCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Product ID is required");

                var productById = await _productRepository.GetProductByIdAsync(
                    deactivateProductCommand.Id,
                    cancellationToken);

                if (productById == null)
                    return Result<Guid>.Failure("Product not found");

                if (productById.IsDeleted)
                    return Result<Guid>.Failure("Product is deleted and cannot be deactivated");

                productById.Deactivate(_currentUser.UserId);

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
                return Result<Guid>.Failure($"Failed to deactivate product: {exception.Message}");
            }
        }
    }
}
