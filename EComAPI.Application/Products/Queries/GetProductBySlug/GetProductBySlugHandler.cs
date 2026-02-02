using EComAPI.Application.Common.Results;
using EComAPI.Application.Products.DTOs;
using EComAPI.Application.Products.Interfaces;

namespace EComAPI.Application.Products.Queries.GetProductBySlug
{
    public class GetProductBySlugHandler
    {
        private readonly IProductRepository _repository;

        public GetProductBySlugHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<ProductDetailDto>> Handle(
            GetProductBySlugQuery query,
            CancellationToken ct)
        {
            var product = await _repository.GetBySlugAsync(query.Slug, ct);
            if (product is null)
                return Result<ProductDetailDto>.Failure("Product not found");

            var dto = ProductDetailDto.From(product);
            return Result<ProductDetailDto>.Success(dto);
        }
    }
}