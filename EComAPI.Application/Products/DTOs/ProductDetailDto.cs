namespace EComAPI.Application.Products.DTOs
{
    public record ProductDetailDto(
        Guid Id,
        string Name,
        string Slug,
        decimal BasePrice,
        string? Description,
        int ViewCount,
        int TotalStock,
        bool IsActive,
        CategoryHierarchyDto? Category,
        IReadOnlyList<ProductVariantDto> Variants,
        IReadOnlyList<ProductImageDto> Images
    );
}