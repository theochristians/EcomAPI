using EComAPI.API.Common;
using EComAPI.API.Products.Dtos.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.GetAllProducts.Response
{
    public class GetAllProductsSuccessExample
        : IExamplesProvider<ApiResponse<IEnumerable<ProductListResponse>>>
    {
        public ApiResponse<IEnumerable<ProductListResponse>> GetExamples()
            => ApiResponse<IEnumerable<ProductListResponse>>.Ok(
                new List<ProductListResponse>
                {
                    new(
                        Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                        "Nike Air Max 90",
                        "nike-air-max-90",
                        1500000,
                        120
                    ),
                    new(
                        Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                        "Adidas Ultraboost",
                        "adidas-ultraboost",
                        1800000,
                        87
                    )
                },
                "Success list product"
            );
    }
}