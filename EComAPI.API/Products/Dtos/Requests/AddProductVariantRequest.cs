namespace EComAPI.API.Products.Dtos.Requests
{
    public record AddProductVariantRequest(
        string Sku,
        int Stock,
        decimal PriceAdjustment = 0,
        string? Size = null,
        string? Color = null
    );
}