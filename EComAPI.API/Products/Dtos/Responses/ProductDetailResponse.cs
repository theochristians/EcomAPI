using EComAPI.Application.Products.DTOs;

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
    )
    {
        public static ProductDetailResponse FromDto(ProductDetailDto productDetailDto)
        {
            return new ProductDetailResponse(
                productDetailDto.Id,
                productDetailDto.Name,
                productDetailDto.Slug,
                productDetailDto.BasePrice,
                productDetailDto.Description,
                productDetailDto.ViewCount,
                productDetailDto.TotalStock,
                productDetailDto.IsActive,
                productDetailDto.Category != null
                    ? MapCategoryHierarchy(productDetailDto.Category)
                    : null,
                productDetailDto.Variants.Select(productVariantDto => new ProductVariantResponse(
                    productVariantDto.Id,
                    productVariantDto.Sku,
                    productVariantDto.Stock,
                    productVariantDto.PriceAdjustment,
                    productVariantDto.FinalPrice,
                    productVariantDto.Size,
                    productVariantDto.Color,
                    productVariantDto.IsActive 
                )).ToList(),
                productDetailDto.Images.Select(productImageDto => new ProductImageResponse(
                    productImageDto.Id,
                    productImageDto.ImageUrl,
                    productImageDto.IsPrimary,
                    productImageDto.DisplayOrder
                )).ToList()
            );
        }
        private static CategoryHierarchyResponse MapCategoryHierarchy(CategoryHierarchyDto categoryHierarchyDto)
        {
            return new CategoryHierarchyResponse(
                categoryHierarchyDto.Id,
                categoryHierarchyDto.Name,
                categoryHierarchyDto.Slug,
                categoryHierarchyDto.Parent != null
                    ? MapCategoryHierarchy(categoryHierarchyDto.Parent)
                    : null
            );
        }
    }

    public record CategoryHierarchyResponse(
        Guid Id,
        string Name,
        string Slug,
        CategoryHierarchyResponse? Parent
    );
}