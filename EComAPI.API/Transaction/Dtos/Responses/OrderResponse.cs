namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record OrderResponse(
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
        List<OrderItemResponse> Items,
        PaymentResponse? Payment,
        List<ReviewResponse> Reviews,
        List<ReturnResponse> Returns);
}
