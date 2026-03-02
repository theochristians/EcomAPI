namespace EComAPI.Application.Products.Commands.ProductCommands.UpdateProduct
{
    public record UpdateProductCommand(
        Guid Id,
        string? Name,          
        string? Slug,          
        decimal? BasePrice,     
        Guid? CategoryId,       
        string? Description   
    );
}