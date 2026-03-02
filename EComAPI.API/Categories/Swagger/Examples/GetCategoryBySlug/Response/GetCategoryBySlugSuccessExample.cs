using EComAPI.API.Categories.Dtos.Responses;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Categories.Swagger.Examples.GetCategoryBySlug.Response
{
    public class GetCategoryBySlugSuccessExample : IExamplesProvider<ApiResponse<CategoryResponse>>
    {
        public ApiResponse<CategoryResponse> GetExamples()
        {
            return ApiResponse<CategoryResponse>.Ok(
                new CategoryResponse(
                    Guid.Parse("c1f7f3a1-8c34-4b12-9b5f-1a2b3c4d5e6f"),
                    "Electronics",
                    "electronics",
                    null,
                    "https://example.com/images/electronics.jpg",
                    "Kategori utama untuk produk elektronik",
                    12,
                    DateTime.Parse("2025-01-10T08:00:00Z"),
                    DateTime.Parse("2025-01-15T10:30:00Z")
                ),
                "Success get category"
            );
        }
    }
}
