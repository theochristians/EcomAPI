namespace EComAPI.Application.Products.DTOs
{
    public record CategoryHierarchyDto(
        Guid Id,
        string Name,
        string Slug,
        string? ImageUrl,
        string? Description,
        CategoryHierarchyDto? Parent
    );
}