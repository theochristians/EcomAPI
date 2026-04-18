using EComAPI.Domain.Transaction.Enums;

namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record CreateOrderRequest(
        Guid AddressId,
        ShippingType ShippingType,
        List<CreateOrderItemRequest> Items,
        string? CouponCode = null,
        string? CustomerNote = null);
}
