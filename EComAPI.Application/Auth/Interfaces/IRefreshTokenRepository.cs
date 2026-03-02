using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface IRefreshTokenRepository
    {
        // =========================
        // QUERY (READ-ONLY)
        // =========================
        Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken = default);

        // =========================
        // COMMAND (WRITE/PERSISTENCE)
        // =========================
        Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    }
}
