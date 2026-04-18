namespace EComAPI.Infrastructure.Common.Caching
{
    public interface ICacheWriteBuffer
    {
        void QueueSetString(string key, string value, DateTime expiresAtUtc);
        void QueueRemove(string key);
        Task FlushAsync(CancellationToken cancellationToken = default);
        void Clear();
    }
}
