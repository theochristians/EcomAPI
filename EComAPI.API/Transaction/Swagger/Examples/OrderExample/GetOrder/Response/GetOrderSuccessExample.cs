using EComAPI.API.Common;
using EComAPI.API.Transaction.Dtos.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Transaction.Swagger.Examples.OrderExample.GetOrder.Response
{
    public class GetOrderSuccessExample : IExamplesProvider<ApiResponse<OrderResponse>>
    {
        public ApiResponse<OrderResponse> GetExamples()
            => ApiResponse<OrderResponse>.Ok(
                new OrderResponse(
                    Id: Guid.Parse("11223344-5566-7788-99aa-bbccddeeff00"),
                    OrderNumber: "INV-20260305-A1B2C3",
                    Status: "pending_payment",
                    ShippingRecipientName: "Theo Christian",
                    ShippingPhone: "081234567890",
                    ShippingFullAddress: "Jl. Sudirman No. 1, RT 01/RW 01",
                    ShippingCity: "Jakarta Pusat",
                    ShippingPostalCode: "10220",
                    TotalAmount: 300000,
                    ShippingCost: 25000,
                    DiscountAmount: 50000,
                    FinalAmount: 275000,
                    Courier: null,
                    TrackingNumber: null,
                    CustomerNote: "Tolong dibungkus dengan rapi",
                    AdminNote: null,
                    CreatedAt: DateTime.UtcNow,
                    Items: new List<OrderItemResponse>
                    {
                        new OrderItemResponse(
                            Id: Guid.Parse("aabbccdd-eeff-0011-2233-445566778899"),
                            ProductId: Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                            ProductVariantId: Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890abc"),
                            SnapshotProductName: "Kaos Polos Premium",
                            SnapshotVariantName: "M / Putih",
                            SnapshotPrice: 150000,
                            Quantity: 2,
                            Subtotal: 300000)
                    },
                    Payment: new PaymentResponse(
                        Id: Guid.Parse("ff001122-3344-5566-7788-99aabbccddee"),
                        PaymentMethod: null,
                        Amount: 275000,
                        Status: "pending",
                        ProofImageUrl: null,
                        AdminNote: null,
                        ConfirmedAt: null),
                    Reviews: new List<ReviewResponse>(),
                    Returns: new List<ReturnResponse>()),
                "Success get order");
    }
}
