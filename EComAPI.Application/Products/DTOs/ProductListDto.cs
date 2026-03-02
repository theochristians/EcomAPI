namespace EComAPI.Application.Products.DTOs
{
    public record ProductListDto(
        Guid Id,
        string Name,
        string Slug,
        decimal BasePrice,
        int ViewCount,
        bool IsActive,
        bool HasStock,        
        int TotalStock,       
        CategoryHierarchyDto? Category
    );
}