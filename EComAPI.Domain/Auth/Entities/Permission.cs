using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Domain.Auth.Entities
{
    public sealed class Permission : BaseEntity
    {
        public string Name { get; private set; }
        public string Category { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }

        private Permission() { }

        public Permission(string name, string category, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Permission name is required");

            if (string.IsNullOrWhiteSpace(category))
                throw new DomainException("Permission category is required");

            Name = name.Trim();
            Category = category.Trim();
            Description = description;
            IsActive = true;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}