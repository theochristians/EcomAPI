using EComAPI.Domain.Product.Entities;

namespace EComAPI.Application.Products.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
        Task AddAsync(Product product, CancellationToken cancellationToken);
        Task UpdateAsync(Product product, CancellationToken cancellationToken);
        Task<Product?> GetByIdIncludeDeletedAsync(Guid id, CancellationToken ct);
    }
}