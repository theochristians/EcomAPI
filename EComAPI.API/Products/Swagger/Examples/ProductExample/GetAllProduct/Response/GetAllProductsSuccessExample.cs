using EComAPI.API.Common;
using EComAPI.API.Products.Dtos.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductExample.GetAllProduct.Response
{
    public class GetAllProductsSuccessExample
        : IExamplesProvider<PaginatedApiResponse<IReadOnlyList<ProductListResponse>>>
    {
        public PaginatedApiResponse<IReadOnlyList<ProductListResponse>> GetExamples()
            => new PaginatedApiResponse<IReadOnlyList<ProductListResponse>>(
                success: true,
                message: "Success list products",
                data: new List<ProductListResponse>
                {
                    new(
                        Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                        "Nike Air Max 90",
                        "nike-air-max-90",
                        1500000,
                        120,
                        true,
                        true,
                        50,
                        new CategoryHierarchyResponse(
                            Guid.Parse("c1f7f3a1-8c34-4b12-9b5f-1a2b3c4d5e6f"),
                            "Shoes",
                            "shoes",
                            null
                        )
                    )
                },
                totalCount: 35,
                currentPage: 1,
                totalPages: 4,
                pageSize: 10,
                hasPreviousPage: false,
                hasNextPage: true,
                previousPageUrl: null,
                nextPageUrl: "https://localhost:7091/api/products?page=2&pageSize=10",
                firstPageUrl: null,
                lastPageUrl: "https://localhost:7091/api/products?page=4&pageSize=10"
            );
    }
}
