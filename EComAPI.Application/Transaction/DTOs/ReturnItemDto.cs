namespace EComAPI.Application.Transaction.DTOs
{
    public record ReturnItemDto(
        Guid Id,
        Guid OrderItemId,
        int Quantity,
        string? Condition,
        string? AdminNote);
}
