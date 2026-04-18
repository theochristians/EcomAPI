using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Interfaces
{
    public interface IStockLogRepository
    {
        Task<IReadOnlyList<StockLog>> GetLogsByVariantIdAsync(Guid productVariantId, CancellationToken cancellationToken = default);
        Task AddLogAsync(StockLog stockLog, CancellationToken cancellationToken = default);
    }
}
