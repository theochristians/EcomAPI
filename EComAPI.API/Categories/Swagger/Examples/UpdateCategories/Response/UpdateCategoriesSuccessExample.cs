using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Categories.Swagger.Examples.UpdateCategories.Response
{
    public class UpdateCategoriesSuccessExample
        : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(
                new { id = Guid.Parse("c1f7f3a1-8c34-4b12-9b5f-1a2b3c4d5e6f") },
                "Category updated successfully"
            );
    }
}