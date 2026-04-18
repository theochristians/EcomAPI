namespace EComAPI.Application.Shopping.DTOs
{
    public record WishlistItemDto(
        Guid Id,
        Guid ProductId,
        string ProductName,
        string ProductSlug,
        decimal BasePrice,
        string? PrimaryImageUrl,
        bool IsActive,
        DateTime AddedAt
    );
}
