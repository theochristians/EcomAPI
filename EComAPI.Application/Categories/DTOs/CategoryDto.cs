namespace EComAPI.Application.Categories.DTOs
{
    public record CategoryDto(
        Guid Id,
        string Name,
        string Slug,
        Guid? ParentId,
        string? ImageUrl,
        string? Description,
        int ProductCount,
        DateTime CreatedAt,
        Guid CreatedBy,
        DateTime? UpdatedAt,
        Guid? UpdatedBy
    );
}