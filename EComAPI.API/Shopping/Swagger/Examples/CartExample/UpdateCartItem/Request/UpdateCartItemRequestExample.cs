using EComAPI.API.Shopping.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Shopping.Swagger.Examples.CartExample.UpdateCartItem.Request
{
    public class UpdateCartItemRequestExample : IExamplesProvider<UpdateCartItemRequest>
    {
        public UpdateCartItemRequest GetExamples()
            => new UpdateCartItemRequest(3);
    }
}
