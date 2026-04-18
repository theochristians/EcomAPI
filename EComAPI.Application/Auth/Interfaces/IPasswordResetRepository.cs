using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface IPasswordResetRepository
    {
        Task<PasswordReset?> GetLatestPendingByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<PasswordReset?> GetByUserIdAndTokenHashAsync(
            Guid userId,
            string tokenHash,
            CancellationToken cancellationToken = default);

        Task AddPasswordResetAsync(
            PasswordReset passwordReset,
            CancellationToken cancellationToken = default);

        Task UpdatePasswordResetAsync(
            PasswordReset passwordReset,
            CancellationToken cancellationToken = default);
    }
}
