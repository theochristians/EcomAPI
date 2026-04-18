using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface ILoginHistoryRepository
    {
        Task AddLoginHistoryAsync(LoginHistory loginHistory, CancellationToken cancellationToken = default);
    }
}
