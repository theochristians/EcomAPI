namespace EComAPI.Application.Products.DTOs
{
    public record ProductVariantDto(
        Guid Id,
        string Sku,
        int Stock,
        decimal PriceAdjustment,
        decimal FinalPrice,
        string? Size,
        string? Color,
        bool IsActive
    );
}