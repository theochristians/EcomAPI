using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class EmailVerificationRepository : IEmailVerificationRepository
    {
        private readonly AppDbContext _appDbContext;

        public EmailVerificationRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<EmailVerification?> GetLatestPendingByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.EmailVerifications
                .Where(emailVerification =>
                    emailVerification.UserId == userId &&
                    emailVerification.VerifiedAt == null)
                .OrderByDescending(emailVerification => emailVerification.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddEmailVerificationAsync(
            EmailVerification emailVerification,
            CancellationToken cancellationToken = default)
        {
            await _appDbContext.EmailVerifications.AddAsync(emailVerification, cancellationToken);
        }

        public Task UpdateEmailVerificationAsync(
            EmailVerification emailVerification,
            CancellationToken cancellationToken = default)
        {
            _appDbContext.EmailVerifications.Update(emailVerification);
            return Task.CompletedTask;
        }
    }
}
