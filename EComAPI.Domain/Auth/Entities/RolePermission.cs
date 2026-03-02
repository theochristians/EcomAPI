using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.Entities
{
    public class RolePermission : BaseEntity
    {
        public Guid RoleId { get; private set; }
        public Guid PermissionId { get; private set; }
        public Role? Role { get; private set; }
        public Permission? Permission { get; private set; }

        private RolePermission() { }

        public RolePermission(Guid roleId, Guid permissionId, Guid createdBy)
        {
            Guard.AgainstEmptyGuid(roleId, "RoleId is required");
            Guard.AgainstEmptyGuid(permissionId, "PermissionId is required");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");
            
            RoleId = roleId;
            PermissionId = permissionId;
            SetCreated(createdBy);
        }
    }
}