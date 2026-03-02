namespace EComAPI.Application.Categories.Commands.UpdateCategories
{
    public record UpdateCategoryCommand(
        Guid Id,
        string? Name,
        string? Slug,
        Guid? ParentId,
        string? ImageUrl,
        string? Description
    );
}