namespace EComAPI.Infrastructure.Common.Caching
{
    public interface ICacheInvalidationBuffer
    {
        void MarkNamespaceDirty(string namespaceName);
        Task FlushAsync(CancellationToken cancellationToken = default);
        void Clear();
    }
}
