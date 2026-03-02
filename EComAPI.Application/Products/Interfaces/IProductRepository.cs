using EComAPI.Domain.Products.Entities;

namespace EComAPI.Application.Products.Interfaces
{
    public interface IProductRepository
    {
        // ==========================================
        // BASIC CRUD OPERATIONS - PRODUCT
        // ==========================================
        Task<Product?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Product?> GetProductByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Product?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<Product?> GetProductByIdWithVariantsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Product?> GetProductByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Product?> GetProductByIdWithAllDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> IncrementProductViewAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddProductAsync(Product product, CancellationToken cancellationToken = default);
        Task UpdateProductAsync(Product product, CancellationToken cancellationToken = default);

        // ==========================================
        // EXISTENCE CHECKS - PRODUCT
        // ==========================================
        Task<bool> ProductExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);

        // ==========================================
        // SPECIALIZED BUSINESS QUERIES - PRODUCT
        // ==========================================
        Task<(IReadOnlyList<Product> Items, int TotalCount)> GetProductsPaginatedAsync(
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
            CancellationToken cancellationToken = default);

        // ==========================================
        // BASIC CRUD OPERATIONS - PRODUCT VARIANT
        // ==========================================
        Task<ProductVariant?> GetProductVariantByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ProductVariant?> GetProductVariantByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddProductVariantAsync(ProductVariant variant, CancellationToken cancellationToken = default);
        Task UpdateProductVariantAsync(ProductVariant variant, CancellationToken cancellationToken = default);

        // ==========================================
        // BASIC CRUD OPERATIONS - PRODUCT IMAGE
        // ==========================================
        Task<ProductImage?> GetProductImageByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ProductImage?> GetProductImageByIdIncludeDeletedAsync(Guid id, CancellationToken cancellationToken = default);
        Task AddProductImageAsync(ProductImage image, CancellationToken cancellationToken = default);
        Task UpdateProductImageAsync(ProductImage image, CancellationToken cancellationToken = default);
    }
}
