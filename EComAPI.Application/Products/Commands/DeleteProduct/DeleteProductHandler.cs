using EComAPI.Application.Common.Results;
using EComAPI.Application.Products.Interfaces;

namespace EComAPI.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductHandler
    {
        private readonly IProductRepository _repository;

        public DeleteProductHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Guid>> Handle(
            DeleteProductCommand command,
            CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(command.Id, cancellationToken);

            if (product == null)
                return Result<Guid>.Failure("Product not found");

            if (product.IsDeleted)
                return Result<Guid>.Failure("Product already deleted");

            product.SoftDelete(null);

            await _repository.UpdateAsync(product, cancellationToken);

            return Result<Guid>.Success(product.Id);
        }
    }
}