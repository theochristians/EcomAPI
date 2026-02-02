using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface IPermissionRepository
    {
        Task<Permission?> GetByIdAsync(Guid id);
        Task<Permission?> GetByNameAsync(string name);
        Task<List<Permission>> GetByRoleIdAsync(Guid roleId);
        Task AddAsync(Permission permission);
        Task<IReadOnlyList<Permission>> GetByUserIdAsync(Guid userId);
    }
}