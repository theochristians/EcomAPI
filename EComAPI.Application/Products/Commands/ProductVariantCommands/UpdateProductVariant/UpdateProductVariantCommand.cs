namespace EComAPI.Application.Products.Commands.ProductVariantCommands.UpdateProductVariant
{
    public record UpdateProductVariantCommand(
        Guid Id,
        string? Size,
        string? Color,
        decimal? PriceAdjustment,
        string? Sku,
        int? Stock
    );
}