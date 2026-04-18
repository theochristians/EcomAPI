using System.Collections.Concurrent;
using EComAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EComAPI.Infrastructure.Common.Caching
{
    public sealed class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<string, long> _namespaceVersions = new(StringComparer.OrdinalIgnoreCase);

        public InMemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(key, out T? cachedValue))
                return Task.FromResult(cachedValue);

            return Task.FromResult(default(T));
        }

        public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            if (ttl <= TimeSpan.Zero)
                return Task.CompletedTask;

            _memoryCache.Set(key, value, ttl);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        public Task<long> GetNamespaceVersionAsync(string namespaceName, CancellationToken cancellationToken = default)
        {
            var version = _namespaceVersions.GetOrAdd(namespaceName, 1);
            return Task.FromResult(version);
        }

        public Task<IReadOnlyDictionary<string, long>> GetNamespaceVersionsAsync(
            IEnumerable<string> namespaceNames,
            CancellationToken cancellationToken = default)
        {
            var versions = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

            foreach (var namespaceName in namespaceNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(namespaceName))
                    continue;

                versions[namespaceName] = _namespaceVersions.GetOrAdd(namespaceName, 1);
            }

            return Task.FromResult<IReadOnlyDictionary<string, long>>(versions);
        }

        public Task<long> IncrementNamespaceVersionAsync(string namespaceName, CancellationToken cancellationToken = default)
        {
            var version = _namespaceVersions.AddOrUpdate(namespaceName, 2, (_, current) => current + 1);
            return Task.FromResult(version);
        }
    }
}
