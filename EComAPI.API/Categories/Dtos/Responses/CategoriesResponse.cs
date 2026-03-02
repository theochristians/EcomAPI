using EComAPI.Application.Categories.DTOs;

namespace EComAPI.API.Categories.Dtos.Responses
{
    public record CategoryResponse(
        Guid Id,
        string Name,
        string Slug,
        Guid? ParentId,
        string? ImageUrl,        // ⭐ NEW
        string? Description,     // ⭐ NEW
        int ProductCount,        // ⭐ NEW
        DateTime CreatedAt,
        DateTime? UpdatedAt
    )
    {
        public static CategoryResponse FromDto(CategoriesDTOs dto)
        {
            return new CategoryResponse(
                dto.Id,
                dto.Name,
                dto.Slug,
                dto.ParentId,
                dto.ImageUrl,
                dto.Description,
                dto.ProductCount,
                dto.CreatedAt,
                dto.UpdatedAt
            );
        }
    }
}