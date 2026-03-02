namespace EComAPI.Application.Products.Commands.ProductImageCommands.UpdateProductImage
{
    public record UpdateProductImageCommand(
        Guid Id,
        string? ImageUrl,
        bool? IsPrimary,
        int? DisplayOrder
    );
}
