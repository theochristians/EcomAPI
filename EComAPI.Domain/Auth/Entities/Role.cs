using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; private set; }
        private readonly List<User> _users = new();
        public IReadOnlyCollection<User> Users => _users;
        private readonly List<RolePermission> _rolePermissions = new();
        public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

        private Role() { }

        public Role(string name, Guid createdBy)
        {
            var nameRoleValue = Guard.AgainstNullOrWhiteSpace(name, "Role name is required");
            Guard.AgainstMaxLength(nameRoleValue, 100, "Role name cannot exceed 100 characters");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            Name = nameRoleValue;
            SetCreated(createdBy);
        }

        public static Role CreateForSeed(Guid id, string name, Guid createdBy)
        {
            Guard.AgainstEmptyGuid(id, "Role ID is required");

            var nameRoleSeedValue = Guard.AgainstNullOrWhiteSpace(name, "Role name is required");
            Guard.AgainstMaxLength(nameRoleSeedValue, 100, "Role name cannot exceed 100 characters");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            var role = new Role
            {
                Id = id,
                Name = nameRoleSeedValue
            };

            role.SetCreated(createdBy);

            return role;
        }

        public void AddPermission(RolePermission rolePermission, Guid updatedBy)
        {
            Guard.AgainstNull(rolePermission, "RolePermission is required");

            if (rolePermission.RoleId != Id)
                throw new DomainException("Role permission does not belong to this role");

            if (_rolePermissions.Any(rp => rp.PermissionId == rolePermission.PermissionId))
                throw new DomainException("Permission already assigned to this role");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            _rolePermissions.Add(rolePermission);
            SetUpdated(updatedBy);
        }

        public void RemovePermission(Guid permissionId, Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(permissionId, "PermissionId is required");
            var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
            if (rolePermission == null)
                throw new DomainException("Permission not found in this role");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");
            _rolePermissions.Remove(rolePermission);
            SetUpdated(updatedBy);
        }
    }
}