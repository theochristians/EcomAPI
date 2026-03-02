using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductExample.UpdateProduct.Response
{
    public class UpdateProductSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(
                new { productId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890") },
                "Product updated successfully"
            );
    }
}
