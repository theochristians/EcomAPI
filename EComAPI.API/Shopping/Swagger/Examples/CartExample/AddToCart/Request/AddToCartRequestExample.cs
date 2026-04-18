using EComAPI.API.Shopping.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Swagger.Examples.CartExample.AddToCart.Request
{
    public class AddToCartRequestExample : IExamplesProvider<AddToCartRequest>
    {
        public AddToCartRequest GetExamples()
            => new AddToCartRequest(
                Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123"),
                2
            );
    }
}
