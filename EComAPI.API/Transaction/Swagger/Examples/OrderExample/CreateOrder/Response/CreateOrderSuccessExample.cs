using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Transaction.Swagger.Examples.OrderExample.CreateOrder.Response
{
    public class CreateOrderSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(
                new { orderNumber = "INV-20260305-A1B2C3" },
                "Order created successfully");
    }
}
