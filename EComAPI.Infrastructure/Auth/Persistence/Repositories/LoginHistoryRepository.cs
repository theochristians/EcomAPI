using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class LoginHistoryRepository : ILoginHistoryRepository
    {
        private readonly AppDbContext _appDbContext;

        public LoginHistoryRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddLoginHistoryAsync(LoginHistory loginHistory, CancellationToken cancellationToken = default)
        {
            await _appDbContext.LoginHistories.AddAsync(loginHistory, cancellationToken);
        }
    }
}
