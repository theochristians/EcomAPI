using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Swagger.Examples.WishlistExample
{
    public class WishlistItemNotFoundExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Fail("Wishlist item not found");
    }
}
