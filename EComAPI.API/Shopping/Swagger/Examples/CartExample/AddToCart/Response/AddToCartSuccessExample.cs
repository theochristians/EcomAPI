using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Swagger.Examples.CartExample.AddToCart.Response
{
    public class AddToCartSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(
                new { cartItemId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012") },
                "Item added to cart"
            );
    }
}
