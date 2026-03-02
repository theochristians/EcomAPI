namespace EComAPI.Application.Common.Interfaces
{
    /// <summary>
    /// Unit of Work pattern untuk transaction management
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Simpan semua perubahan ke database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begin transaction
        /// </summary>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commit transaction
        /// </summary>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rollback transaction
        /// </summary>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}