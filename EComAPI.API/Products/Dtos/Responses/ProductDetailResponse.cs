namespace EComAPI.API.Products.Dtos.Responses
{
    public record ProductDetailResponse(
        Guid Id,
        string Name,
        string Slug,
        decimal BasePrice,
        string? Description,
        int ViewCount,
        int TotalStock,
        bool IsActive,
        CategoryHierarchyResponse? Category,
        IReadOnlyList<ProductVariantResponse> ProductVariantResponses,
        IReadOnlyList<ProductImageResponse> ProductImageResponses
    );
}