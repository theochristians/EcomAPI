using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Product.Entities;
using EComAPI.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Products.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Product product, CancellationToken cancellationToken)
        {
            await _context.Products.AddAsync(product, cancellationToken);
        }

        public Task UpdateAsync(Product product, CancellationToken cancellationToken)
        {
            _context.Products.Update(product);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .ToListAsync(cancellationToken);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        {
            return await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
        }

        public async Task<Product?> GetByIdIncludeDeletedAsync(Guid id, CancellationToken ct)
        {
            return await _context.Products
                .IgnoreQueryFilters()
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }
    }
}
