namespace EComAPI.Application.Products.Commands.ProductImageCommands.AddProductImage
{
    public record AddProductImageCommand(
        Guid ProductId,
        string ImageUrl,
        bool IsPrimary = false,
        int DisplayOrder = 0
    );
}