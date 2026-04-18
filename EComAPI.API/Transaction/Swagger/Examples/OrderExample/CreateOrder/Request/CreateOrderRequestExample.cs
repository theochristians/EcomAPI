using EComAPI.API.Transaction.Dtos.Requests;
using EComAPI.Domain.Transaction.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Transaction.Swagger.Examples.OrderExample.CreateOrder.Request
{
    public class CreateOrderRequestExample : IExamplesProvider<CreateOrderRequest>
    {
        public CreateOrderRequest GetExamples()
            => new CreateOrderRequest(
                AddressId: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                ShippingType: ShippingType.Regular,
                Items: new List<CreateOrderItemRequest>
                {
                    new CreateOrderItemRequest(
                        ProductVariantId: Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890abc"),
                        Quantity: 2)
                },
                CouponCode: "DISKON20",
                CustomerNote: "Tolong dibungkus dengan rapi");
    }
}
