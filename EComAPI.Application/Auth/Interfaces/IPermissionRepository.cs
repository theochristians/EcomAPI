using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface IPermissionRepository
    {
        // QUERY (READ-ONLY)
        Task<Permission?> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Permission?> GetPermissionByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Permission>> GetPermissionByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Permission>> GetPermissionByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        // COMMAND (WRITE/PERSISTENCE)
        Task AddPermissionAsync(Permission permission, CancellationToken cancellationToken = default);
    }
}
