using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Interfaces
{
    public interface IOrderStatusLogRepository
    {
        Task<IReadOnlyList<OrderStatusLog>> GetLogsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
        Task AddLogAsync(OrderStatusLog orderStatusLog, CancellationToken cancellationToken = default);
    }
}
