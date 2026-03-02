using EComAPI.Domain.Categories.Entities;

namespace EComAPI.Application.Categories.Interfaces
{
    public interface ICategoryRepository
    {
        // ==========================================
        // BASIC CRUD OPERATIONS
        // ==========================================
        Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Category?> GetCategoryByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Category?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
        Task AddCategoryAsync(Category category, CancellationToken cancellationToken = default);
        Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default);

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