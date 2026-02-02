using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Categories.Swagger.Examples.UpdateCategory.Response
{
    public class UpdateCategoryBadRequestExample
        : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Fail("Category update failed");
    }
}