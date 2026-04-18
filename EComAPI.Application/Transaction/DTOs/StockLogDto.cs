namespace EComAPI.Application.Transaction.DTOs
{
    public record StockLogDto(
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
