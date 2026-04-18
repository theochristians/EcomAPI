using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface ITokenBlacklistRepository
    {
        // QUERY (READ-ONLY)
        Task<bool> IsBlacklistedAsync(string tokenHash, CancellationToken cancellationToken = default);

        // COMMAND (WRITE/PERSISTENCE)
        Task AddTokenBlacklistAsync(TokenBlacklist tokenBlacklist, CancellationToken cancellationToken = default);
        Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
}
