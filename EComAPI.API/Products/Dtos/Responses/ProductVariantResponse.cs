namespace EComAPI.API.Products.Dtos.Responses
{
    public record ProductVariantResponse(
        Guid Id,
        string Sku,
        int Stock,
        decimal PriceAdjustment,
        string? Size,
        string? Color
    );
}