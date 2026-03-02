namespace EComAPI.API.Products.Dtos.Responses
{
    public record ProductVariantResponse(
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