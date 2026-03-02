using EComAPI.Application.Products.DTOs;

namespace EComAPI.API.Products.Dtos.Responses
{
    public record ProductListResponse(
        Guid Id,
        string Name,
        string Slug,
        decimal BasePrice,
        int ViewCount,
        bool IsActive,
        bool HasStock,         
        int TotalStock,        
        CategoryHierarchyResponse? Category
    )
    {
        public static ProductListResponse FromDto(ProductListDto dto)
        {
            return new ProductListResponse(
                dto.Id,
                dto.Name,
                dto.Slug,
                dto.BasePrice,
                dto.ViewCount,
                dto.IsActive,
                dto.HasStock,
                dto.TotalStock,
                dto.Category != null
                    ? MapCategoryHierarchy(dto.Category)
                    : null
            );
        }
        private static CategoryHierarchyResponse MapCategoryHierarchy(CategoryHierarchyDto dto)
        {
            return new CategoryHierarchyResponse(
                dto.Id,
                dto.Name,
                dto.Slug,
                dto.Parent != null
                    ? MapCategoryHierarchy(dto.Parent)
                    : null
            );
        }
    }
}