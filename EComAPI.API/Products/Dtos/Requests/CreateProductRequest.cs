namespace EComAPI.API.Products.Dtos.Requests
{
    public record CreateProductRequest(
        Guid CategoryId,
        string Name,
        string Slug,
        decimal BasePrice,
        string? Description
    );
}