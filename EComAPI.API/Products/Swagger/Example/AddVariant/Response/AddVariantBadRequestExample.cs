using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.AddVariant.Response
{
    public class AddVariantBadRequestExample
        : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Fail("Product variant failed add");
    }
}