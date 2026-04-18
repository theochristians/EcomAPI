namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record ApproveReturnRequest(
        decimal RefundAmount,
        List<ApproveReturnItemRequest>? ItemConditions = null);
}
