namespace EComAPI.API.Categories.Dtos.Responses
{
    public record CategoryResponse(
        Guid Id,
        string Name,
        string Slug,
        Guid? ParentId,
        DateTime CreatedAt,
        Guid? CreatedBy,
        DateTime? UpdatedAt,
        Guid? UpdatedBy
    );
}