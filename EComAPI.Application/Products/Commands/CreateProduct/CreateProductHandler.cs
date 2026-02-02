using EComAPI.Application.Common.Results;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Domain.Product.Entities;

namespace EComAPI.Application.Products.Commands.CreateProduct
{
    public class CreateProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateProductHandler(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<Result<Guid>> Handle(
            CreateProductCommand command,
            CancellationToken cancellationToken)
        {
            var categoryExists = await _categoryRepository.ExistsAsync(
                command.CategoryId,
                cancellationToken);

            if (!categoryExists)
                return Result<Guid>.Failure("Category not found");

            var product = new Product(
                command.CategoryId,
                command.Name,
                command.Slug,
                command.BasePrice,
                command.Description
            );

            await _productRepository.AddAsync(product, cancellationToken);

            return Result<Guid>.Success(product.Id);
        }
    }
}