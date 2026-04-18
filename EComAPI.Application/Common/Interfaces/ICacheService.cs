namespace EComAPI.Application.Common.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task<long> GetNamespaceVersionAsync(string namespaceName, CancellationToken cancellationToken = default);
        Task<IReadOnlyDictionary<string, long>> GetNamespaceVersionsAsync(
            IEnumerable<string> namespaceNames,
            CancellationToken cancellationToken = default);
        Task<long> IncrementNamespaceVersionAsync(string namespaceName, CancellationToken cancellationToken = default);
    }
}
