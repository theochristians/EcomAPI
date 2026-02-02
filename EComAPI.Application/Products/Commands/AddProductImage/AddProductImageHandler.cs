using EComAPI.Application.Common.Results;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Product.Entities;
using System.Threading;

namespace EComAPI.Application.Products.Commands.AddProductImage
{
    public class AddProductImageHandler
    {
        private readonly IProductRepository _repository;

        public AddProductImageHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(
            AddProductImageCommand command,
            CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(command.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure("Product not found");

            var image = new ProductImage(
                product.Id,
                command.ImageUrl,
                command.IsPrimary,
                command.DisplayOrder
            );

            product.AddImage(image);
            await _repository.UpdateAsync(product, cancellationToken);
            return Result.Success();
        }
    }
}