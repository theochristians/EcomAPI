using EComAPI.Application.Common.Interfaces;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace EComAPI.Infrastructure.Common.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext appDbContext;
        private IDbContextTransaction? _dnContextTransaction;

        public UnitOfWork(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await appDbContext.SaveChangesAsync(cancellationToken);
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
        }
    }
}