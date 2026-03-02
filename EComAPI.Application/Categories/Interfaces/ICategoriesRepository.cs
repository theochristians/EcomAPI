using EComAPI.Domain.Categories.Entities;

namespace EComAPI.Application.Categories.Interfaces
{
    public interface ICategoryRepository
    {
        // ==========================================
        // BASIC CRUD OPERATIONS
        // ==========================================
        Task<Domain.Categories.Entities.Categories?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Domain.Categories.Entities.Categories?> GetCategoryByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Domain.Categories.Entities.Categories?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Domain.Categories.Entities.Categories>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
        Task AddCategoryAsync(Domain.Categories.Entities.Categories categories, CancellationToken cancellationToken = default);
        Task UpdateCategoryAsync(Domain.Categories.Entities.Categories categories, CancellationToken cancellationToken = default);

        // ==========================================
        // EXISTENCE CHECKS
        // ==========================================
        Task<bool> CategoryExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> CategoryExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

        // ==========================================
        // SPECIALIZED BUSINESS QUERIES
        // ==========================================
        Task<int> GetProductCountAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, int>> GetProductCountsAsync(IEnumerable<Guid> categoryIds, CancellationToken cancellationToken = default);
        Task<bool> HasActiveProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<bool> HasActiveSubcategoriesAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<int> GetActiveSubcategoriesCountAsync(Guid categoryId, CancellationToken cancellationToken = default);
    }
}