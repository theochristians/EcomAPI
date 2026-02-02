namespace EComAPI.API.Products.Dtos.Requests
{
    public record AddProductVariantRequest(
        string Sku,
        int Stock,
        decimal PriceAdjustment,
        string? Size,
        string? Color
    );
}