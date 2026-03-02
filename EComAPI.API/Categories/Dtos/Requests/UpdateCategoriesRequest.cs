namespace EComAPI.API.Categories.Dtos.Requests
{
    public record UpdateCategoryRequest(
        string? Name,
        string? Slug,
        Guid? ParentId,
        string? ImageUrl,        // ⭐ NEW
        string? Description      // ⭐ NEW
    );
}