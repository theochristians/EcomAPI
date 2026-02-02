namespace EComAPI.API.Products.Dtos.Responses
{
    public record ProductListResponse(
        Guid Id,
        string Name,
        string Slug,
        decimal BasePrice,
        int ViewCount
    );
}