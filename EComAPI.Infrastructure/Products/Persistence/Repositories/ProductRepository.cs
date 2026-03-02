using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Products.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Products.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _appDbContext;

        public ProductRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // ==========================================
        // BASIC CRUD OPERATIONS - PRODUCT
        // ==========================================
        public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Products
                .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        }

        public async Task<Product?> GetProductByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _appDbContext.Products
                .IgnoreQueryFilters()
                .Include(product => product.Variants)
                .Include(product => product.Images)
                .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

            if (product != null)
            {
                product.RecalculateTotalStock();
            }

            return product;
        }

        public async Task<Product?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var product = await _appDbContext.Products
                .Include(product => product.Variants)
                .Include(product => product.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(product => product.Slug == slug, cancellationToken);

            if (product != null)
            {
                product.RecalculateTotalStock();
            }

            return product;
        }

        public async Task<Product?> GetProductByIdWithVariantsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _appDbContext.Products
                .Include(product => product.Variants)
                .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

            if (product != null)
            {
                product.RecalculateTotalStock();
            }

            return product;
        }

        public async Task<Product?> GetProductByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Products
                .Include(product => product.Images)
                .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
        }

        public async Task<Product?> GetProductByIdWithAllDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var product = await _appDbContext.Products
                .Include(product => product.Variants)
                .Include(product => product.Images)
                .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

            if (product != null)
            {
                product.RecalculateTotalStock();
            }

            return product;
        }

        public async Task<bool> IncrementProductViewAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _appDbContext.Products
                .Where(product => product.Id == id)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(product => product.ViewCount, product => product.ViewCount + 1)
                    .SetProperty(product => product.UpdatedAt, _ => DateTime.UtcNow),
                    cancellationToken);

            return affectedRows > 0;
        }

        public async Task AddProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Products.AddAsync(product, cancellationToken);
        }

        public Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            _appDbContext.Products.Update(product);
            return Task.CompletedTask;
        }

        // ==========================================
        // EXISTENCE CHECKS - PRODUCT
        // ==========================================
        public async Task<bool> ProductExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Products
                .AnyAsync(product => product.Slug == slug, cancellationToken);
        }

        // ==========================================
        // SPECIALIZED BUSINESS QUERIES - PRODUCT
        // ==========================================
        public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsPaginatedAsync(
            List<Guid>? categoryIds,
            decimal? minPrice,
            decimal? maxPrice,
            bool? inStock,
            bool? isActive,
            string? searchTerm,
            string sortBy,
            string sortOrder,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // Start with base query
            var query = _appDbContext.Products
                .Include(product => product.Variants)
                .AsQueryable();

            // Filter by categories
            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(product => categoryIds.Contains(product.CategoryId));
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(product => product.BasePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(product => product.BasePrice <= maxPrice.Value);
            }

            // Filter by stock status
            if (inStock.HasValue)
            {
                if (inStock.Value)
                {
                    query = query.Where(product => product.TotalStock > 0);
                }
                else
                {
                    query = query.Where(product => product.TotalStock == 0);
                }
            }

            // Filter by active status
            if (isActive.HasValue)
            {
                query = query.Where(product => product.IsActive == isActive.Value);
            }

            // Search by name
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(product => product.Name.ToLower().Contains(lowerSearchTerm));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(product => product.Name)
                    : query.OrderByDescending(product => product.Name),

                "price" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(product => product.BasePrice)
                    : query.OrderByDescending(product => product.BasePrice),

                "viewcount" => sortOrder.ToLower() == "asc"
                    ? query.OrderBy(product => product.ViewCount)
                    : query.OrderByDescending(product => product.ViewCount),

                _ => sortOrder.ToLower() == "asc"  // default: createdAt
                    ? query.OrderBy(product => product.CreatedAt)
                    : query.OrderByDescending(product => product.CreatedAt)
            };

            // Apply pagination
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Recalculate TotalStock for each product
            foreach (var product in products)
            {
                product.RecalculateTotalStock();
            }

            return (products, totalCount);
        }

        // ==========================================
        // BASIC CRUD OPERATIONS - PRODUCT VARIANT
        // ==========================================
        public async Task<ProductVariant?> GetProductVariantByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.ProductVariants
                .FirstOrDefaultAsync(productVariant => productVariant.Id == id, cancellationToken);
        }

        public async Task<ProductVariant?> GetProductVariantByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.ProductVariants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(productVariant => productVariant.Id == id, cancellationToken);
        }

        public async Task AddProductVariantAsync(ProductVariant productVariant, CancellationToken cancellationToken = default)
        {
            await _appDbContext.ProductVariants.AddAsync(productVariant, cancellationToken);
        }

        public Task UpdateProductVariantAsync(ProductVariant productVariant, CancellationToken cancellationToken = default)
        {
            _appDbContext.ProductVariants.Update(productVariant);
            return Task.CompletedTask;
        }

        // ==========================================
        // BASIC CRUD OPERATIONS - PRODUCT IMAGE
        // ==========================================
        public async Task<ProductImage?> GetProductImageByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.ProductImages
                .FirstOrDefaultAsync(productImage => productImage.Id == id, cancellationToken);
        }

        public async Task<ProductImage?> GetProductImageByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.ProductImages
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(productImage => productImage.Id == id, cancellationToken);
        }

        public async Task AddProductImageAsync(ProductImage productImage, CancellationToken cancellationToken = default)
        {
            await _appDbContext.ProductImages.AddAsync(productImage, cancellationToken);
        }

        public Task UpdateProductImageAsync(ProductImage productImage, CancellationToken cancellationToken = default)
        {
            _appDbContext.ProductImages.Update(productImage);
            return Task.CompletedTask;
        }
    }
}
