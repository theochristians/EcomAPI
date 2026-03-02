namespace EComAPI.Application.Products.Commands.ProductCommands.UpdateProductStock
{
    public record UpdateProductStockCommand(
        Guid VariantId,
        int Stock
    );
}