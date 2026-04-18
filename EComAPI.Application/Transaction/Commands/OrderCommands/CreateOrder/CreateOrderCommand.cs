using EComAPI.Domain.Transaction.Enums;

namespace EComAPI.Application.Transaction.Commands.OrderCommands.CreateOrder
{
    public record CreateOrderCommand(
        Guid UserId,
        Guid AddressId,
        ShippingType ShippingType,
        List<CreateOrderItemDto> Items,
        string? CouponCode = null,
        string? CustomerNote = null);

    public record CreateOrderItemDto(
        Guid ProductVariantId,
        int Quantity);
}
