using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _appDbContext;

        public PermissionRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Permission?> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(permission => permission.Id == id, cancellationToken);
        }

        public async Task<Permission?> GetPermissionByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(permission => permission.Name == name, cancellationToken);
        }

        public async Task<IReadOnlyList<Permission>> GetPermissionByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.RolePermissions
                .Where(rolePermission => rolePermission.RoleId == roleId)
                .Join(
                    _appDbContext.Permissions,
                    rolePermission => rolePermission.PermissionId,
                    permission => permission.Id,
                    (_, permission) => permission
                )
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Permission>> GetPermissionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Users
                .Where(user => user.Id == userId)
                .Join(
                    _appDbContext.RolePermissions,
                    user => user.RoleId,
                    rolePermission => rolePermission.RoleId,
                    (_, rolePermission) => rolePermission.PermissionId
                )
                .Join(
                    _appDbContext.Permissions,
                    permissionId => permissionId,
                    permission => permission.Id,
                    (_, permission) => permission
                )
                .Where(permission => permission.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddPermissionAsync(Permission permission, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Permissions.AddAsync(permission, cancellationToken);
        }
    }
}