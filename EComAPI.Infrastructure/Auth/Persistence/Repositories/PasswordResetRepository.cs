using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly AppDbContext _appDbContext;

        public PasswordResetRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<PasswordReset?> GetLatestPendingByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.PasswordResets
                .Where(passwordReset =>
                    passwordReset.UserId == userId &&
                    passwordReset.UsedAt == null &&
                    passwordReset.DeletedAt == null)
                .OrderByDescending(passwordReset => passwordReset.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<PasswordReset?> GetByUserIdAndTokenHashAsync(
            Guid userId,
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.PasswordResets
                .Where(passwordReset =>
                    passwordReset.UserId == userId &&
                    passwordReset.TokenHash == tokenHash &&
                    passwordReset.UsedAt == null &&
                    passwordReset.DeletedAt == null)
                .OrderByDescending(passwordReset => passwordReset.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task AddPasswordResetAsync(
            PasswordReset passwordReset,
            CancellationToken cancellationToken = default)
        {
            await _appDbContext.PasswordResets.AddAsync(passwordReset, cancellationToken);
        }

        public Task UpdatePasswordResetAsync(
            PasswordReset passwordReset,
            CancellationToken cancellationToken = default)
        {
            _appDbContext.PasswordResets.Update(passwordReset);
            return Task.CompletedTask;
        }
    }
}
