using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.GetAllProducts.Response
{
    public class GetAllProductsBadRequestExample
        : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Fail("Failed list product");
    }
}