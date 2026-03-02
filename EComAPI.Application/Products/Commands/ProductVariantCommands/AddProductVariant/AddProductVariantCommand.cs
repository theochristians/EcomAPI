namespace EComAPI.Application.Products.Commands.ProductVariantCommands.AddProductVariant
{
    public record AddProductVariantCommand(
        Guid ProductId,
        string Sku,
        int Stock,
        decimal PriceAdjustment = 0,
        string? Size = null,
        string? Color = null
    );
}