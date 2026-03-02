namespace EComAPI.API.Categories.Dtos.Requests
{
    public record CreateCategoryRequest(
        string Name,
        string Slug,
        Guid? ParentId,
        string? ImageUrl,        // ⭐ NEW
        string? Description      // ⭐ NEW
    );
}