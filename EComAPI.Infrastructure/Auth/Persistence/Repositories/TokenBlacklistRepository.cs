using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Caching;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class TokenBlacklistRepository : ITokenBlacklistRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly ICacheService _cacheService;
        private readonly ICacheWriteBuffer _cacheWriteBuffer;

        public TokenBlacklistRepository(
            AppDbContext appDbContext,
            ICacheService cacheService,
            ICacheWriteBuffer cacheWriteBuffer)
        {
            _appDbContext = appDbContext;
            _cacheService = cacheService;
            _cacheWriteBuffer = cacheWriteBuffer;
        }

        public async Task AddTokenBlacklistAsync(TokenBlacklist tokenBlacklist, CancellationToken cancellationToken = default)
        {
            await _appDbContext.TokenBlacklists.AddAsync(tokenBlacklist, cancellationToken);

            if (tokenBlacklist.ExpiresAt > SecurityTime.UtcNow)
            {
                _cacheWriteBuffer.QueueSetString(
                    GetBlacklistCacheKey(tokenBlacklist.TokenHash),
                    "1",
                    tokenBlacklist.ExpiresAt);
            }
        }

        public async Task<bool> IsBlacklistedAsync(string tokenHash, CancellationToken cancellationToken = default)
        {
            var cacheKey = GetBlacklistCacheKey(tokenHash);
            var cachedValue = await _cacheService.GetAsync<string>(cacheKey, cancellationToken);

            if (!string.IsNullOrWhiteSpace(cachedValue))
                return true;

            var now = SecurityTime.UtcNow;

            var blacklistedToken = await _appDbContext.TokenBlacklists
                .AsNoTracking()
                .Where(tokenBlacklist => tokenBlacklist.TokenHash == tokenHash && tokenBlacklist.ExpiresAt > now)
                .OrderByDescending(tokenBlacklist => tokenBlacklist.ExpiresAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (blacklistedToken == null)
                return false;

            var ttl = blacklistedToken.ExpiresAt - now;
            if (ttl > TimeSpan.Zero)
            {
                await _cacheService.SetAsync(cacheKey, "1", ttl, cancellationToken);
            }

            return true;
        }

        public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var now = SecurityTime.UtcNow;

            var expiredTokens = await _appDbContext.TokenBlacklists
                .Where(tokenBlacklist => tokenBlacklist.ExpiresAt <= now)
                .ToListAsync(cancellationToken);

            foreach (var expiredToken in expiredTokens)
            {
                _cacheWriteBuffer.QueueRemove(GetBlacklistCacheKey(expiredToken.TokenHash));
            }

            _appDbContext.TokenBlacklists.RemoveRange(expiredTokens);
        }

        private static string GetBlacklistCacheKey(string tokenHash)
            => $"{CacheKeys.TokenBlacklistPrefix}{tokenHash}";
    }
}
