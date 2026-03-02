using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface IRoleRepository
    {
        // =========================
        // QUERY (READ-ONLY)
        // =========================
        Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Role?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
