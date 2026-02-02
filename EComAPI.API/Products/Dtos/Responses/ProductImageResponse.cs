namespace EComAPI.API.Products.Dtos.Responses
{
    public record ProductImageResponse(
        Guid Id,
        string ImageUrl,
        bool IsPrimary,
        int DisplayOrder
    );
}