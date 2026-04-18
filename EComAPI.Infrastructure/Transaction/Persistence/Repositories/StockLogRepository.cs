using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Transaction.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Transaction.Persistence.Repositories
{
    public class StockLogRepository : IStockLogRepository
    {
        private readonly AppDbContext _appDbContext;

        public StockLogRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IReadOnlyList<StockLog>> GetLogsByVariantIdAsync(
            Guid productVariantId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.StockLogs
                .Where(stockLog => stockLog.ProductVariantId == productVariantId)
                .OrderByDescending(stockLog => stockLog.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddLogAsync(StockLog log, CancellationToken cancellationToken = default)
        {
            await _appDbContext.StockLogs.AddAsync(log, cancellationToken);
        }
    }
}
