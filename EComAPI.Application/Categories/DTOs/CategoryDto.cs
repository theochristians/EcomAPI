using EComAPI.Domain.Categories.Entities;

namespace EComAPI.Application.Categories.DTOs
{
    public class CategoryDto
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Slug { get; }
        public Guid? ParentId { get; }
        public DateTime CreatedAt { get; }
        public Guid? CreatedBy { get; }
        public DateTime? UpdatedAt { get; }
        public Guid? UpdatedBy { get; }

        private CategoryDto(
            Guid id,
            string name,
            string slug,
            Guid? parentId,
            DateTime createdAt,
            Guid? createdBy,
            DateTime? updatedAt,
            Guid? updatedBy)
        {
            Id = id;
            Name = name;
            Slug = slug;
            ParentId = parentId;
            CreatedAt = createdAt;
            CreatedBy = createdBy;
            UpdatedAt = updatedAt;
            UpdatedBy = updatedBy;
        }

        public static CategoryDto From(Category category)
        {
            return new CategoryDto(
                category.Id,
                category.Name,
                category.Slug,
                category.ParentId,
                category.CreatedAt,
                category.CreatedBy,
                category.UpdatedAt,
                category.UpdatedBy
            );
        }
    }

}