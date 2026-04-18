using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Swagger.Examples.WishlistExample.AddToWishlist.Response
{
    public class AddToWishlistSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(
                new { wishlistItemId = Guid.Parse("e5f6a7b8-c9d0-1234-efab-567890123456") },
                "Product added to wishlist"
            );
    }
}
