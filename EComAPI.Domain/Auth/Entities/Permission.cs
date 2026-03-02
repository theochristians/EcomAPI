using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.Entities
{
    public sealed class Permission : BaseEntity
    {
        public string Name { get; private set; }
        public string Category { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }

        private readonly List<RolePermission> _rolePermissions = new();
        public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

        private Permission() { }

        public Permission(string name, string category, Guid createdBy, string? description = null)
        {
            var namePermissionValue = Guard.AgainstNullOrWhiteSpace(name, "Permission name is required");
            Guard.AgainstMaxLength(namePermissionValue, 150, "Permission name cannot exceed 150 characters");

            var categoryPermissionValue = Guard.AgainstNullOrWhiteSpace(category, "Permission category is required");
            Guard.AgainstMaxLength(categoryPermissionValue, 100, "Permission category cannot exceed 100 characters");

            var descriptionPermissionValue = Guard.AgainstMaxLengthIfProvided(description, 500, "Description cannot exceed 500 characters");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            Name = namePermissionValue;
            Category = categoryPermissionValue;
            Description = descriptionPermissionValue;
            IsActive = true;
            SetCreated(createdBy);
        }

        public static Permission CreateForSeed(
            Guid id,
            string name,
            string category,
            Guid createdBy,
            string? description = null)
        {
            Guard.AgainstEmptyGuid(id, "Permission ID is required");

            var namePermissionValue = Guard.AgainstNullOrWhiteSpace(name, "Permission name is required");
            Guard.AgainstMaxLength(namePermissionValue, 150, "Permission name cannot exceed 150 characters");

            var categoryPermissionValue = Guard.AgainstNullOrWhiteSpace(category, "Permission category is required");
            Guard.AgainstMaxLength(categoryPermissionValue, 100, "Permission category cannot exceed 100 characters");

            var descriptionPermissionValue = Guard.AgainstMaxLengthIfProvided(description, 500, "Description cannot exceed 500 characters");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            var permission = new Permission
            {
                Id = id,
                Name = namePermissionValue,
                Category = categoryPermissionValue,
                Description = descriptionPermissionValue,
                IsActive = true
            };

            permission.SetCreated(createdBy);

            return permission;
        }

        public void Update(string name, string category, Guid updatedBy, string? description = null)
        {
            var namePermissionValue = Guard.AgainstNullOrWhiteSpace(name, "Permission name is required");
            Guard.AgainstMaxLength(namePermissionValue, 150, "Permission name cannot exceed 150 characters");

            var categoryPermissionValue = Guard.AgainstNullOrWhiteSpace(category, "Permission category is required");
            Guard.AgainstMaxLength(categoryPermissionValue, 100, "Permission category cannot exceed 100 characters");

            var descriptionPermissionValue = Guard.AgainstMaxLengthIfProvided(description, 500, "Description cannot exceed 500 characters");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Name = namePermissionValue;
            Category = categoryPermissionValue;
            Description = descriptionPermissionValue;
            SetUpdated(updatedBy);
        }

        public void Activate(Guid updatedBy)
        {
            if (IsActive)
                throw new DomainException("Permission is already active");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            IsActive = true;
            SetUpdated(updatedBy);
        }

        public void Deactivate(Guid updatedBy)
        {
            if (!IsActive)
                throw new DomainException("Permission is already inactive");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            IsActive = false;
            SetUpdated(updatedBy);
        }
    }
}