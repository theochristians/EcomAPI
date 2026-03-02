using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductExample.RestoreProduct.Response
{
    public class RestoreProductSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(
                new { productId = Guid.Parse("f0e1d2c3-b4a5-6789-0abc-def123456789") },
                "Product restored success"
            );
    }
}
