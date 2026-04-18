namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record OrderItemResponse(
        Guid Id,
        Guid ProductId,
        Guid ProductVariantId,
        string SnapshotProductName,
        string SnapshotVariantName,
        decimal SnapshotPrice,
        int Quantity,
        decimal Subtotal);
}
