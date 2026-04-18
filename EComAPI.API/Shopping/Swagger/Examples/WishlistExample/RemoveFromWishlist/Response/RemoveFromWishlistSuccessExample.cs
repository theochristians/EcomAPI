using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Swagger.Examples.WishlistExample.RemoveFromWishlist.Response
{
    public class RemoveFromWishlistSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(
                new { wishlistItemId = Guid.Parse("e5f6a7b8-c9d0-1234-efab-567890123456") },
                "Product removed from wishlist"
            );
    }
}
