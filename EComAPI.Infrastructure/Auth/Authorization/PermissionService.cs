using EComAPI.Application.Auth.Interfaces;
using Microsoft.EntityFrameworkCore;
using EComAPI.Infrastructure.Common.Persistence;

namespace EComAPI.Infrastructure.Auth.Authorization
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string permission)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Join(_context.Roles, u => u.RoleId, r => r.Id, (_, r) => r)
                .Join(_context.RolePermissions, r => r.Id, rp => rp.RoleId, (_, rp) => rp)
                .Join(_context.Permissions, rp => rp.PermissionId, p => p.Id, (_, p) => p)
                .AnyAsync(p => p.Name == permission);
        }
    }
}