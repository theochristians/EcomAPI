namespace EComAPI.Application.Categories.Commands.CreateCategories
{
    public record CreateCategoryCommand(
        string Name,
        string Slug,
        Guid? ParentId,
        string? ImageUrl,
        string? Description
    );
}