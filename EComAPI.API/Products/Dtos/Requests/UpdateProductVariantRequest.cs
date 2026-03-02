namespace EComAPI.API.Products.Dtos.Requests
{
    public record UpdateProductVariantRequest(
        string? Sku,
        int? Stock,
        decimal? PriceAdjustment,
        string? Size,
        string? Color
    );
}
