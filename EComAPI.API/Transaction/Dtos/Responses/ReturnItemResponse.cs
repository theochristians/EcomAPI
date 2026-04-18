namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record ReturnItemResponse(
        Guid Id,
        Guid OrderItemId,
        int Quantity,
        string? Condition,
        string? AdminNote);
}
