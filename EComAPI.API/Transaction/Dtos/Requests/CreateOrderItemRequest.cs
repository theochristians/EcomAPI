namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record CreateOrderItemRequest(
        Guid ProductVariantId,
        int Quantity);
}
