using EComAPI.API.Common;
using EComAPI.API.Shopping.Dtos.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Swagger.Examples.WishlistExample.GetWishlist.Response
{
    public class GetWishlistSuccessExample : IExamplesProvider<ApiResponse<IReadOnlyList<WishlistItemResponse>>>
    {
        public ApiResponse<IReadOnlyList<WishlistItemResponse>> GetExamples()
            => ApiResponse<IReadOnlyList<WishlistItemResponse>>.Ok(
                new List<WishlistItemResponse>
                {
                    new WishlistItemResponse(
                        Guid.Parse("e5f6a7b8-c9d0-1234-efab-567890123456"),
                        Guid.Parse("f6a7b8c9-d0e1-2345-fabc-678901234567"),
                        "Kaos Polos Premium",
                        "kaos-polos-premium",
                        150000m,
                        "https://example.com/images/kaos-polos-premium.jpg",
                        true,
                        DateTime.Parse("2025-01-20T10:00:00Z")
                    )
                },
                "Success get wishlist"
            );
    }
}
