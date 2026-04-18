namespace EComAPI.Application.Transaction.DTOs
{
    public record OrderItemDto(
        Guid Id,
        Guid ProductId,
        Guid ProductVariantId,
        string SnapshotProductName,
        string SnapshotVariantName,
        decimal SnapshotPrice,
        int Quantity);
}
