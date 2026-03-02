using EComAPI.Application.Categories.Interfaces;
using EComAPI.Domain.Categories.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Categories.Persistence.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _appDbContext;

        public CategoryRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddCategoryAsync(Domain.Categories.Entities.Categories categories, CancellationToken cancellationToken)
        {
            await _appDbContext.Categories.AddAsync(categories, cancellationToken);
        }

        public async Task<bool> CategoryExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _appDbContext.Categories
                .AnyAsync(category => category.Id == id, cancellationToken);
        }

        public async Task<bool> CategoryExistsBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return await _appDbContext.Categories
                .AnyAsync(category => category.Slug == slug, cancellationToken);
        }

        public async Task<IReadOnlyList<Domain.Categories.Entities.Categories>> GetAllCategoriesAsync(CancellationToken cancellationToken)
        {
            return await _appDbContext.Categories
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Domain.Categories.Entities.Categories?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _appDbContext.Categories
                .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        }

        public async Task<Domain.Categories.Entities.Categories?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return await _appDbContext.Categories
                .FirstOrDefaultAsync(category => category.Slug == slug, cancellationToken);
        }

        public async Task<Domain.Categories.Entities.Categories?> GetCategoryByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _appDbContext.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        }

        public Task UpdateCategoryAsync(Domain.Categories.Entities.Categories categories, CancellationToken cancellationToken)
        {
            _appDbContext.Categories.Update(categories);
            // ❌ NO SaveChangesAsync - use UoW
            return Task.CompletedTask;
        }

        public async Task<int> GetProductCountAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            // Get all subcategory IDs (recursive)
            var categoryIds = await GetCategoryIdsRecursiveAsync(categoryId, cancellationToken);

            // Add parent category
            categoryIds.Add(categoryId);

            // Count products in all these categories
            var count = await _appDbContext.Products
                .Where(product => categoryIds.Contains(product.CategoryId) && product.DeletedAt == null)
                .CountAsync(cancellationToken);

            return count;
        }

        // ⭐ NEW - Get product counts for multiple categories (optimized)
        public async Task<Dictionary<Guid, int>> GetProductCountsAsync(
            IEnumerable<Guid> categoryIds,
            CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<Guid, int>();

            foreach (var categoryId in categoryIds)
            {
                var count = await GetProductCountAsync(categoryId, cancellationToken);
                result[categoryId] = count;
            }

            return result;
        }

        // ⭐ HELPER - Get all subcategory IDs recursively
        private async Task<List<Guid>> GetCategoryIdsRecursiveAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            var result = new List<Guid>();

            // Get direct children
            var children = await _appDbContext.Categories
                .Where(category => category.ParentId == categoryId && category.DeletedAt == null)
                .Select(category => category.Id)
                .ToListAsync(cancellationToken);

            result.AddRange(children);

            // Recursively get children of children
            foreach (var childId in children)
            {
                var subChildren = await GetCategoryIdsRecursiveAsync(childId, cancellationToken);
                result.AddRange(subChildren);
            }

            return result;
        }
        public async Task<bool> HasActiveProductsAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            // Check products in this category
            var hasProducts = await _appDbContext.Products
                .AnyAsync(product => product.CategoryId == categoryId && product.DeletedAt == null, cancellationToken);

            if (hasProducts)
                return true;

            // Check products in subcategories (recursive)
            var subcategoryIds = await GetCategoryIdsRecursiveAsync(categoryId, cancellationToken);

            if (subcategoryIds.Any())
            {
                hasProducts = await _appDbContext.Products
                    .AnyAsync(product => subcategoryIds.Contains(product.CategoryId) && product.DeletedAt == null, cancellationToken);
            }

            return hasProducts;
        }

        // ⭐ NEW: Check if category has active subcategories
        public async Task<bool> HasActiveSubcategoriesAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Categories
                .AnyAsync(category => category.ParentId == categoryId && category.DeletedAt == null, cancellationToken);
        }

        // ⭐ NEW: Get count of active subcategories
        public async Task<int> GetActiveSubcategoriesCountAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Categories
                .CountAsync(category => category.ParentId == categoryId && category.DeletedAt == null, cancellationToken);
        }
    }
}