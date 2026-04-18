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
    );
}