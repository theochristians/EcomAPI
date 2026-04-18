using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Interfaces
{
    public interface IReturnRepository
    {
        Task<Return?> GetReturnByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Return?> GetReturnWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Return> Items, int TotalCount)> GetReturnsByUserIdAsync(
            Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<bool> ReturnExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task AddReturnAsync(Return returnEntity, CancellationToken cancellationToken = default);
        Task UpdateReturnAsync(Return returnEntity, CancellationToken cancellationToken = default);
        Task<string> GenerateReturnNumberAsync(CancellationToken cancellationToken = default);
        Task AddReturnImageAsync(ReturnImage returnImage, CancellationToken cancellationToken = default);
    }
}
