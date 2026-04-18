namespace EComAPI.API.Products.Dtos.Responses
{
    public record CategoryHierarchyResponse(
        Guid Id,
        string Name,
        string Slug,
        CategoryHierarchyResponse? Parent
    );
}
