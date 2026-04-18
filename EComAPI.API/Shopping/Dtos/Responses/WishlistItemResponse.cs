namespace EComAPI.API.Shopping.Dtos.Responses
{
    public record WishlistItemResponse(
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
