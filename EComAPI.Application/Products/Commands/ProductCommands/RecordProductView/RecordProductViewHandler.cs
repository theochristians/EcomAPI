using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Products.Commands.ProductCommands.RecordProductView
{
    public class RecordProductViewHandler
    {
        private readonly IProductRepository _productRepository;

        public RecordProductViewHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<Result> Handle(
            RecordProductViewCommand recordProductViewCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (recordProductViewCommand.ProductId == Guid.Empty)
                    return Result.Failure("Product ID is required");

                var isUpdated = await _productRepository.IncrementProductViewAsync(
                    recordProductViewCommand.ProductId,
                    cancellationToken);

                return isUpdated
                    ? Result.Success()
                    : Result.Failure("Product not found");
            }
            catch (DomainException domainException)
            {
                return Result.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result.Failure($"Failed to record product view: {exception.Message}");
            }
        }
    }
}
