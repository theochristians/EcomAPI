using Microsoft.EntityFrameworkCore;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _appDbContext;

        public RefreshTokenRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await _appDbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.RefreshTokens
                .Include(refreshToken => refreshToken.User)
                    .ThenInclude(user => user!.Role)
                .FirstOrDefaultAsync(refreshToken => refreshToken.TokenHash == tokenHash, cancellationToken);
        }

        public async Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = SecurityTime.UtcNow;

            return await _appDbContext.RefreshTokens
                .Where(refreshToken =>
                    refreshToken.UserId == userId &&
                    refreshToken.RevokedAt == null &&
                    refreshToken.ExpiresAt > now)
                .ToListAsync(cancellationToken);
        }

        public Task UpdateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            _appDbContext.RefreshTokens.Update(refreshToken);
            return Task.CompletedTask;
        }
    }
}