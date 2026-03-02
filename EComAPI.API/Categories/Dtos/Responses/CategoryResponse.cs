namespace EComAPI.API.Categories.Dtos.Responses
{
    public record CategoryResponse(
        Guid Id,
        string Name,
        string Slug,
        Guid? ParentId,
        string? ImageUrl,
        string? Description,
        int ProductCount,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}