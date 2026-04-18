using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Transaction.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Transaction.Persistence.Repositories
{
    public class ReturnRepository : IReturnRepository
    {
        private readonly AppDbContext _appDbContext;

        public ReturnRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Return?> GetReturnByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Returns
                .FirstOrDefaultAsync(returnEntity => returnEntity.Id == id, cancellationToken);
        }

        public async Task<Return?> GetReturnWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Returns
                .Include(returnEntity => returnEntity.Items)
                .Include(returnEntity => returnEntity.Images)
                .FirstOrDefaultAsync(returnEntity => returnEntity.Id == id, cancellationToken);
        }

        public async Task<(IReadOnlyList<Return> Items, int TotalCount)> GetReturnsByUserIdAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _appDbContext.Returns
                .Where(returnEntity => returnEntity.UserId == userId)
                .AsQueryable();

            var totalCount = await query.CountAsync(cancellationToken);

            var returns = await query
                .OrderByDescending(returnEntity => returnEntity.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(returnEntity => returnEntity.Items)
                .Include(returnEntity => returnEntity.Images)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (returns, totalCount);
        }

        public async Task<bool> ReturnExistsForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Returns
                .AnyAsync(returnEntity => returnEntity.OrderId == orderId, cancellationToken);
        }

        public async Task AddReturnAsync(Return returnEntity, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Returns.AddAsync(returnEntity, cancellationToken);
        }

        public async Task UpdateReturnAsync(Return returnEntity, CancellationToken cancellationToken = default)
        {
            _appDbContext.Returns.Update(returnEntity);
            await Task.CompletedTask;
        }

        public async Task<string> GenerateReturnNumberAsync(CancellationToken cancellationToken = default)
        {
            var datePrefix = $"RET-{JakartaTime.Now:yyyyMMdd}";
            var todayReturns = await _appDbContext.Returns
                .CountAsync(returnEntity => returnEntity.ReturnNumber.StartsWith(datePrefix), cancellationToken);

            var sequence = (todayReturns + 1).ToString("D3");
            return $"{datePrefix}-{sequence}";
        }

        public async Task AddReturnImageAsync(ReturnImage returnImage, CancellationToken cancellationToken = default)
        {
            await _appDbContext.ReturnImages.AddAsync(returnImage, cancellationToken);
        }
    }
}
