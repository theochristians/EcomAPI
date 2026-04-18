namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record StockLogResponse(
        Guid Id,
        Guid ProductVariantId,
        string Type,
        int QuantityChange,
        int StockBefore,
        int StockAfter,
        string? ReferenceType,
        Guid? ReferenceId,
        string? Note,
        DateTime CreatedAt,
        Guid? CreatedBy);
}
