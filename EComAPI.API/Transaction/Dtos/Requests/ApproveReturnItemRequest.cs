namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record ApproveReturnItemRequest(
        Guid ReturnItemId,
        string Condition,
        string? AdminNote = null);
}
