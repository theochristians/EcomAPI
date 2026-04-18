using EComAPI.API.Shopping.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Swagger.Examples.WishlistExample.AddToWishlist.Request
{
    public class AddToWishlistRequestExample : IExamplesProvider<AddToWishlistRequest>
    {
        public AddToWishlistRequest GetExamples()
            => new AddToWishlistRequest(
                Guid.Parse("f6a7b8c9-d0e1-2345-fabc-678901234567")
            );
    }
}
