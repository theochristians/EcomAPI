using EComAPI.Domain.Categories.Entities;

namespace EComAPI.Application.Categories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
        Task AddAsync(Category category, CancellationToken cancellationToken);
        Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken);
        Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);
        Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Category?> GetByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateAsync(Category category, CancellationToken cancellationToken);
    }
}