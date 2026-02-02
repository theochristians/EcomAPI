using EComAPI.Domain.Common.Base;

namespace EComAPI.Domain.Categories.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public Guid? ParentId { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        protected Category() { }

        public Category(string name, string slug, Guid? parentId = null)
        {
            Name = name;
            Slug = slug;
            ParentId = parentId;
        }

        public void SoftDelete()
        {
            DeletedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            string slug,
            Guid? parentId)
        {
            Name = name;
            Slug = slug;
            ParentId = parentId;
            SetUpdated(null);
        }
        public void Restore()
        {
            DeletedAt = null;
            SetUpdated(null);
        }
    }
}