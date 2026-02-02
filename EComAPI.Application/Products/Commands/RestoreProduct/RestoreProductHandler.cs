using EComAPI.Application.Common.Results;
using EComAPI.Application.Products.Interfaces;

namespace EComAPI.Application.Products.Commands.RestoreProduct
{
    public class RestoreProductHandler
    {
        private readonly IProductRepository _repository;

        public RestoreProductHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(
            RestoreProductCommand command,
            CancellationToken cancellationToken)
        {
            var product = await _repository
                .GetByIdIncludeDeletedAsync(command.Id, cancellationToken);

            if (product == null)
                return Result<Guid>.Failure("Product not found");

            if (!product.IsDeleted)
                return Result<Guid>.Failure("Product is not deleted");

            product.Restore();

            await _repository.UpdateAsync(product, cancellationToken);

            return Result<Guid>.Success(product.Id);
        }
    }
}