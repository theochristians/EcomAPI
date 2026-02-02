using EComAPI.Application.Common.Results;
using EComAPI.Application.Products.DTOs;
using EComAPI.Application.Products.Interfaces;

namespace EComAPI.Application.Products.Queries.GetProducts
{
    public class GetProductsHandler
    {
        private readonly IProductRepository _repository;

        public GetProductsHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<IReadOnlyList<ProductListDto>>> Handle(
            GetProductsQuery query,
            CancellationToken ct)
        {
            var products = await _repository.GetAllAsync(ct);

            var result = products.Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.Slug,
                p.BasePrice,
                p.ViewCount
            )).ToList();

            return Result<IReadOnlyList<ProductListDto>>.Success(result);
        }
    }
}