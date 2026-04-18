namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record CreateReturnItemRequest(
        Guid OrderItemId,
        int Quantity);
}
