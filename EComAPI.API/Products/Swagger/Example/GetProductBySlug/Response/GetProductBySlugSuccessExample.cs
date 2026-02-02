using EComAPI.API.Common;
using EComAPI.API.Products.Dtos.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.GetProductBySlug.Response
{
    public class GetProductBySlugSuccessExample
        : IExamplesProvider<ApiResponse<ProductDetailResponse>>
    {
        public ApiResponse<ProductDetailResponse> GetExamples()
            => ApiResponse<ProductDetailResponse>.Ok(
                new ProductDetailResponse(
                    Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                    "Nike Air Max 90",
                    "nike-air-max-90",
                    1500000,
                    "Sepatu lari klasik dengan teknologi Air Max untuk kenyamanan maksimal.",
                    new List<ProductVariantResponse>
                    {
                        new(
                            Guid.Parse("d4e5f6a7-b8c9-0123-defg-456789012345"),
                            "NIKE-AIRMAX90-42-BLK",
                            50,
                            200000,
                            "42",
                            "Black"
                        ),
                        new(
                            Guid.Parse("e5f6a7b8-c9d0-1234-efgh-567890123456"),
                            "NIKE-AIRMAX90-43-WHT",
                            30,
                            200000,
                            "43",
                            "White"
                        )
                    },
                    new List<ProductImageResponse>
                    {
                        new(
                            Guid.Parse("f6a7b8c9-d0e1-2345-fghi-678901234567"),
                            "https://cdn.example.com/images/nike-airmax90-main.jpg",
                            true,
                            1
                        ),
                        new(
                            Guid.Parse("a7b8c9d0-e1f2-3456-ghij-789012345678"),
                            "https://cdn.example.com/images/nike-airmax90-side.jpg",
                            false,
                            2
                        )
                    }
                ),
                "Product detail success"
            );
    }
}