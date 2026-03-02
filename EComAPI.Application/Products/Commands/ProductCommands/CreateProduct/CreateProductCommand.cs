namespace EComAPI.Application.Products.Commands.ProductCommands.CreateProduct
{
    public record CreateProductCommand(
        Guid CategoryId,
        string Name,
        string Slug,
        decimal BasePrice,
        string? Description = null
    );
}