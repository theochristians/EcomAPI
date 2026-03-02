namespace EComAPI.Application.Products.DTOs
{
    public record ProductImageDto(
        Guid Id,
        string ImageUrl,
        bool IsPrimary,
        int DisplayOrder
    );
}