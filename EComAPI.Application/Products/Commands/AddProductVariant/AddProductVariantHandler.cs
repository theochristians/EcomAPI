using EComAPI.Application.Common.Results;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Product.Entities;
using System.Threading;

namespace EComAPI.Application.Products.Commands.AddProductVariant
{
    public class AddProductVariantHandler
    {
        private readonly IProductRepository _repository;

        public AddProductVariantHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(
            AddProductVariantCommand command,
            CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(command.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure("Product not found");

            var variant = new ProductVariant(
                product.Id,
                command.Sku,
                command.Stock,
                command.PriceAdjustment,
                command.Size,
                command.Color
            );

            product.AddVariant(variant);
            await _repository.UpdateAsync(product, cancellationToken);
            return Result.Success();
        }
    }
}