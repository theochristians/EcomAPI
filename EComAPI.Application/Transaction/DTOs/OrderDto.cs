namespace EComAPI.Application.Transaction.DTOs
{
    public record OrderDto(
        Guid Id,
        string OrderNumber,
        string Status,
        string ShippingRecipientName,
        string ShippingPhone,
        string ShippingFullAddress,
        string ShippingCity,
        string ShippingPostalCode,
        decimal TotalAmount,
        decimal ShippingCost,
        decimal DiscountAmount,
        decimal FinalAmount,
        string? Courier,
        string? TrackingNumber,
        string? CustomerNote,
        string? AdminNote,
        DateTime CreatedAt,
        List<OrderItemDto> Items,
        PaymentDto? Payment,
        IReadOnlyList<ReviewDto> Reviews,
        IReadOnlyList<ReturnDto> Returns);
}
