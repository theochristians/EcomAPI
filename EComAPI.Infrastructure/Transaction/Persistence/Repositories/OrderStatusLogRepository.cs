using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Transaction.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Transaction.Persistence.Repositories
{
    public class OrderStatusLogRepository : IOrderStatusLogRepository
    {
        private readonly AppDbContext _appDbContext;

        public OrderStatusLogRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IReadOnlyList<OrderStatusLog>> GetLogsByOrderIdAsync(
            Guid orderId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.OrderStatusLogs
                .Where(log => log.OrderId == orderId)
                .OrderByDescending(log => log.ChangedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddLogAsync(OrderStatusLog log, CancellationToken cancellationToken = default)
        {
            await _appDbContext.OrderStatusLogs.AddAsync(log, cancellationToken);
        }
    }
}
