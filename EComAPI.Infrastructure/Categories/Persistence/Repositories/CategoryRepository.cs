using EComAPI.Application.Categories.Interfaces;
using EComAPI.Domain.Categories.Entities;
using EComAPI.Infrastructure.Common.Persistence;
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

        public async Task AddAsync(Category category, CancellationToken cancellationToken)
        {
            await _appDbContext.Categories.AddAsync(category, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            // Global query filter otomatis exclude DeletedAt != null
            return await _appDbContext.Categories
                .AnyAsync(category => category.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            // Global query filter otomatis exclude DeletedAt != null
            return await _appDbContext.Categories
                .AnyAsync(category => category.Slug == slug, cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken)
        {
            // Global query filter otomatis exclude DeletedAt != null
            return await _appDbContext.Categories
                .ToListAsync(cancellationToken);
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            // Global query filter otomatis exclude DeletedAt != null
            return await _appDbContext.Categories
                .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        }

        public async Task<Category?> GetByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken)
        {
            // IgnoreQueryFilters() untuk include soft deleted items
            return await _appDbContext.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(category => category.Id == id, cancellationToken);
        }

        public Task UpdateAsync(Category category, CancellationToken cancellationToken)
        {
            _appDbContext.Categories.Update(category);
            return Task.CompletedTask;
        }
    }
}