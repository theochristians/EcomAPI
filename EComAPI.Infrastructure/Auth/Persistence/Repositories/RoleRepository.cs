using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _appDbContext;

        public RoleRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(role => role.Id == id, cancellationToken);
        }

        public async Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(role => role.Name == name, cancellationToken);
        }
    }
}