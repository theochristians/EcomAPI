using System;
using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Domain.Auth.Entities
{
    public class RolePermission : BaseEntity
    {
        public Guid RoleId { get; private set; }
        public Guid PermissionId { get; private set; }

        private RolePermission()
        {
        }

        public RolePermission(Guid roleId, Guid permissionId)
        {
            if (roleId == Guid.Empty)
                throw new DomainException("RoleId is required");

            if (permissionId == Guid.Empty)
                throw new DomainException("PermissionId is required");

            RoleId = roleId;
            PermissionId = permissionId;
        }
    }
}