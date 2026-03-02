using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface IUserRepository
    {
        // =========================
        // QUERY (READ-ONLY)
        // =========================
        Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsUserAsync(string email, CancellationToken cancellationToken = default);

        // =========================
        // COMMAND (WRITE/PERSISTENCE)
        // =========================
        Task AddUserAsync(User user, CancellationToken cancellationToken = default);
        Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task HardDeleteUserAsync(User user, CancellationToken cancellationToken = default);
    }
}
