using EComAPI.Application.Common.Interfaces;

namespace EComAPI.Infrastructure.Common.Caching
{
    public sealed class CacheInvalidationBuffer : ICacheInvalidationBuffer
    {
        private readonly HashSet<string> _dirtyNamespaces = new(StringComparer.OrdinalIgnoreCase);
        private readonly ICacheService _cacheService;

        public CacheInvalidationBuffer(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public void MarkNamespaceDirty(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                return;

            _dirtyNamespaces.Add(namespaceName.Trim());
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_dirtyNamespaces.Count == 0)
                return;

            var namespacesToInvalidate = _dirtyNamespaces.ToArray();
            _dirtyNamespaces.Clear();

            foreach (var namespaceName in namespacesToInvalidate)
            {
                await _cacheService.IncrementNamespaceVersionAsync(namespaceName, cancellationToken);
            }
        }

        public void Clear() => _dirtyNamespaces.Clear();
    }
}
