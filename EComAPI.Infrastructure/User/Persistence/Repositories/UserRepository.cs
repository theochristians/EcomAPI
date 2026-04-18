using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;

        public UserRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        
        public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Users
                .Include(user => user.Role)
                .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Users
                .Include(user => user.Role)
                .FirstOrDefaultAsync(user => user.Email.Value == email, cancellationToken);
        }

        public async Task<bool> ExistsUserAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Users
                .AnyAsync(user => user.Email.Value == email, cancellationToken);
        }

        public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Users.AddAsync(user, cancellationToken);
        }

        public Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            var userEntry = _appDbContext.Entry(user);

            // User fetched from this DbContext is already tracked.
            // Avoid calling Update(user) because it marks the whole graph as Modified,
            // including newly added LoginHistory entities that should be inserted.
            if (userEntry.State == EntityState.Detached)
            {
                _appDbContext.Users.Attach(user);
                userEntry = _appDbContext.Entry(user);
                userEntry.State = EntityState.Modified;
            }

            return Task.CompletedTask;
        }

        public Task HardDeleteUserAsync(User user, CancellationToken cancellationToken = default)
        {
            _appDbContext.Users.Remove(user);
            return Task.CompletedTask;
        }
    }
}