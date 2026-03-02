using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class TokenBlacklistRepository : ITokenBlacklistRepository
    {
        private readonly AppDbContext _appDbContext;

        public TokenBlacklistRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddTokenBlacklistAsync(TokenBlacklist tokenBlacklist, CancellationToken cancellationToken = default)
        {
            await _appDbContext.TokenBlacklists.AddAsync(tokenBlacklist, cancellationToken);
        }

        public async Task<bool> IsBlacklistedAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _appDbContext.TokenBlacklists
                .AnyAsync(tokenBlacklist => tokenBlacklist.TokenHash == tokenHash && tokenBlacklist.ExpiresAt > now, cancellationToken);
        }

        public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var expiredTokens = await _appDbContext.TokenBlacklists
                .Where(tokenBlacklist => tokenBlacklist.ExpiresAt <= now)
                .ToListAsync(cancellationToken);

            _appDbContext.TokenBlacklists.RemoveRange(expiredTokens);
        }
    }
}