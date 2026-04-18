using EComAPI.Application.Common.Interfaces;
using EComAPI.Infrastructure.Common.Caching;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace EComAPI.Infrastructure.Common.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext appDbContext;
        private readonly ICacheInvalidationBuffer _cacheInvalidationBuffer;
        private readonly ICacheWriteBuffer _cacheWriteBuffer;
        private IDbContextTransaction? _dnContextTransaction;

        public UnitOfWork(
            AppDbContext appDbContext,
            ICacheInvalidationBuffer cacheInvalidationBuffer,
            ICacheWriteBuffer cacheWriteBuffer)
        {
            this.appDbContext = appDbContext;
            _cacheInvalidationBuffer = cacheInvalidationBuffer;
            _cacheWriteBuffer = cacheWriteBuffer;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var affectedRows = await appDbContext.SaveChangesAsync(cancellationToken);
                await _cacheInvalidationBuffer.FlushAsync(cancellationToken);
                await _cacheWriteBuffer.FlushAsync(cancellationToken);
                return affectedRows;
            }
            catch
            {
                _cacheInvalidationBuffer.Clear();
                _cacheWriteBuffer.Clear();
                throw;
            }
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _dnContextTransaction = await appDbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await appDbContext.SaveChangesAsync(cancellationToken);

                if (_dnContextTransaction != null)
                {
                    await _dnContextTransaction.CommitAsync(cancellationToken);
                }

                await _cacheInvalidationBuffer.FlushAsync(cancellationToken);
                await _cacheWriteBuffer.FlushAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_dnContextTransaction != null)
                {
                    await _dnContextTransaction.DisposeAsync();
                    _dnContextTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_dnContextTransaction != null)
            {
                await _dnContextTransaction.RollbackAsync(cancellationToken);
                await _dnContextTransaction.DisposeAsync();
                _dnContextTransaction = null;
            }

            _cacheInvalidationBuffer.Clear();
            _cacheWriteBuffer.Clear();
        }
    }
}
