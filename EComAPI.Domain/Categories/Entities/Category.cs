using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Categories.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public Guid? ParentId { get; private set; }

        public string? ImageUrl { get; private set; }
        public string? Description { get; private set; }

        public Category? Parent { get; private set; }

        private readonly List<Category> _children = new();
        public IReadOnlyCollection<Category> Children => _children;

        private Category() { }

        public Category(
            string name,
            string slug,
            Guid createdBy,
            Guid? parentId = null,
            string? imageUrl = null,
            string? description = null)
        {
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            Name = EnsureName(name);
            Slug = EnsureSlug(slug);
            ParentId = EnsureParentId(parentId, Id);
            ImageUrl = Guard.AgainstMaxLengthIfProvided(imageUrl, 500, "Image URL cannot exceed 500 characters");
            Description = Guard.AgainstMaxLengthIfProvided(description, 1000, "Description cannot exceed 1000 characters");

            SetCreated(createdBy);
        }

        public void Update(
            string name,
            string slug,
            Guid updatedBy,
            Guid? parentId = null,
            string? imageUrl = null,
            string? description = null)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Name = EnsureName(name);
            Slug = EnsureSlug(slug);
            ParentId = EnsureParentId(parentId, Id);
            ImageUrl = Guard.AgainstMaxLengthIfProvided(imageUrl, 500, "Image URL cannot exceed 500 characters");
            Description = Guard.AgainstMaxLengthIfProvided(description, 1000, "Description cannot exceed 1000 characters");

            SetUpdated(updatedBy);
        }

        private static string EnsureName(string name)
        {
            var value = Guard.AgainstNullOrWhiteSpace(name, "Category name is required");
            Guard.AgainstMaxLength(value, 100, "Category name cannot exceed 100 characters");
            return value;
        }

        private static string EnsureSlug(string slug)
        {
            var value = Guard.AgainstNullOrWhiteSpace(slug, "Category slug is required");
            Guard.AgainstMaxLength(value, 100, "Category slug cannot exceed 100 characters");
            return value.ToLowerInvariant();
        }

        private static Guid? EnsureParentId(Guid? parentId, Guid currentCategoryId)
        {
            if (!parentId.HasValue)
                return null;

            Guard.AgainstEmptyGuid(parentId.Value, "ParentId is invalid");

            if (parentId.Value == currentCategoryId)
                throw new DomainException("Category cannot be its own parent");

            return parentId.Value;
        }
    }
}
