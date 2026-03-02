using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductCommands.ActivateProduct
{
    public class ActivateProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public ActivateProductHandler(
            IProductRepository productRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            ActivateProductCommand activateProductCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (activateProductCommand.Id == Guid.Empty)
                    return Result<Guid>.Failure("Product ID is required");

                var productById = await _productRepository.GetProductByIdAsync(
                    activateProductCommand.Id,
                    cancellationToken);

                if (productById == null)
                    return Result<Guid>.Failure("Product not found");

                if (productById.IsDeleted)
                    return Result<Guid>.Failure("Deleted product cannot be activated. Restore first");

                productById.Activate(_currentUser.UserId);

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
                return Result<Guid>.Failure($"Failed to activate product: {exception.Message}");
            }
        }
    }
}
