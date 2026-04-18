using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Swagger.Examples.CartExample.ClearCart.Response
{
    public class ClearCartSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(
                new { cartId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890") },
                "Cart cleared"
            );
    }
}
