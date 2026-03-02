using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface IEmailVerificationRepository
    {
        Task<EmailVerification?> GetLatestPendingByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task AddEmailVerificationAsync(
            EmailVerification emailVerification,
            CancellationToken cancellationToken = default);

        Task UpdateEmailVerificationAsync(
            EmailVerification emailVerification,
            CancellationToken cancellationToken = default);
    }
}
