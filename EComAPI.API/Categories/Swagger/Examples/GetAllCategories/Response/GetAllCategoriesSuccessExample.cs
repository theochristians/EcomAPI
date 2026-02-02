using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Categories.Swagger.Examples.CategoryExample.Response
{
    public class GetAllCategoriesSuccessExample
        : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(
                new
                {
                    product_categories = new List<object>
                    {
                        new
                        {
                            id = Guid.Parse("c1f7f3a1-8c34-4b12-9b5f-1a2b3c4d5e6f"),
                            name = "Electronics",
                            slug = "electronics",
                            parentId = (Guid?)null,
                            createdAt = DateTime.UtcNow.AddDays(-10),
                            createdBy = "admin",
                            updatedAt = DateTime.UtcNow.AddDays(-5),
                            updatedBy = "admin"
                        },
                        new
                        {
                            id = Guid.Parse("d2a8b3c1-9d45-5c23-ac6g-2b3c4d5e6f7a"),
                            name = "Smartphones",
                            slug = "smartphones",
                            parentId = Guid.Parse("c1f7f3a1-8c34-4b12-9b5f-1a2b3c4d5e6f"),
                            createdAt = DateTime.UtcNow.AddDays(-8),
                            createdBy = "admin",
                            updatedAt = DateTime.UtcNow.AddDays(-3),
                            updatedBy = "admin"
                        }
                    }
                },
                "Success list categories"
            );
    }
}