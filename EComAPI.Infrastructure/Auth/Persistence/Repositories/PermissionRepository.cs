using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Permission?> GetByIdAsync(Guid id)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Permission?> GetByNameAsync(string name)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<List<Permission>> GetByRoleIdAsync(Guid roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Join(
                    _context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (_, p) => p
                )
                .ToListAsync();
        }

        public async Task AddAsync(Permission permission)
        {
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
        }
        public async Task<IReadOnlyList<Permission>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Join(
                    _context.RolePermissions,
                    u => u.RoleId,
                    rp => rp.RoleId,
                    (_, rp) => rp.PermissionId
                )
                .Join(
                    _context.Permissions,
                    pid => pid,
                    p => p.Id,
                    (_, p) => p
                )
                .Where(p => p.IsActive)
                .ToListAsync();
        }
    }
}