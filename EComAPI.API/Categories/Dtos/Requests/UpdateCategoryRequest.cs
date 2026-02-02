namespace EComAPI.API.Categories.Dtos.Requests
{
    public record UpdateCategoryRequest(
        string Name,
        string Slug,
        Guid? ParentId
    );
}