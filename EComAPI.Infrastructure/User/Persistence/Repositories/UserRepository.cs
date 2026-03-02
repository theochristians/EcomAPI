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

        #region User Methods

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Users
                .Include(user => user.Role)
                .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Users
                .Include(user => user.Role)
                .FirstOrDefaultAsync(user => user.Email.Value == email, cancellationToken);
        }

        public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Users
                .AnyAsync(user => user.Email.Value == email, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Users.AddAsync(user, cancellationToken);
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _appDbContext.Users.Update(user);
            return Task.CompletedTask;
        }

        public Task HardDeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            _appDbContext.Users.Remove(user);
            return Task.CompletedTask;
        }

        #endregion

        #region RefreshToken Methods

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await _appDbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.RefreshTokens
                .Include(refreshToken => refreshToken.User)
                    .ThenInclude(user => user.Role)
                .FirstOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash, cancellationToken);
        }

        public Task UpdateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            _appDbContext.RefreshTokens.Update(refreshToken);
            return Task.CompletedTask;
        }

        public async Task RevokeAllUserRefreshTokensAsync(
            Guid userId,
            string reason,
            Guid revokedBy,
            CancellationToken cancellationToken = default)
        {
            // ⭐ FIX: Don't use IsExpired computed property
            var now = DateTime.UtcNow;

            var activeTokens = await _appDbContext.RefreshTokens
                .Where(refreshToken => refreshToken.UserId == userId
                                       && !refreshToken.RevokedAt.HasValue
                                       && refreshToken.ExpiresAt > now) 
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.Revoke(reason, revokedBy);
            }
        }
        #endregion
    }
}