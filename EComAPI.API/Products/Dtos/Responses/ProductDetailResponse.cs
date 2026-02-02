namespace EComAPI.API.Products.Dtos.Responses
{
    public record ProductDetailResponse(
        Guid Id,
        string Name,
        string Slug,
        decimal BasePrice,
        string? Description,
        IReadOnlyList<ProductVariantResponse> Variants,
        IReadOnlyList<ProductImageResponse> Images
    );
}