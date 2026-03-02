using EComAPI.API.Common;
using EComAPI.API.Products.Dtos.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductExample.GetProductBySlug.Response
{
    public class GetProductBySlugSuccessExample
        : IExamplesProvider<ApiResponse<ProductDetailResponse>>
    {
        public ApiResponse<ProductDetailResponse> GetExamples()
            => ApiResponse<ProductDetailResponse>.Ok(
                new ProductDetailResponse(
                    Guid.Parse("d4e5f6a7-b8c9-0123-def0-456789012345"),
                    "Nike Air Max 90",
                    "nike-air-max-90",
                    1500000m,
                    "Sepatu lari klasik dengan teknologi Air Max untuk kenyamanan maksimal.",
                    123, // ViewCount
                    80,  // TotalStock
                    true, // IsActive
                    new CategoryHierarchyResponse(
                        Guid.Parse("c1f7f3a1-8c34-4b12-9b5f-1a2b3c4d5e6f"),
                        "Footwear",
                        "footwear",
                        null
                    ),
                    new List<ProductVariantResponse>
                    {
                        new(
                            Guid.Parse("e1a1f6a7-b8c9-4123-abcd-456789012346"),
                            "NIKE-AIRMAX90-42-BLK",
                            50,
                            200000m,    // PriceAdjustment
                            1700000m,   // FinalPrice (BasePrice + PriceAdjustment)
                            "42",
                            "Black",
                            true        // IsActive
                        ),
                        new(
                            Guid.Parse("f2b2f6a7-b8c9-5123-abcd-456789012347"),
                            "NIKE-AIRMAX90-43-WHT",
                            30,
                            200000m,    // PriceAdjustment
                            1700000m,   // FinalPrice
                            "43",
                            "White",
                            true        // IsActive
                        )
                    },
                    new List<ProductImageResponse>
                    {
                        new(
                            Guid.Parse("a3c3f6a7-b8c9-6123-abcd-456789012348"),
                            "https://cdn.example.com/images/nike-airmax90-main.jpg",
                            true,
                            1
                        ),
                        new(
                            Guid.Parse("b4d4f6a7-b8c9-7123-abcd-456789012349"),
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