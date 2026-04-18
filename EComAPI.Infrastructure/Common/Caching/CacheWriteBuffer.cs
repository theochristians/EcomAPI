using EComAPI.Application.Common.Interfaces;

namespace EComAPI.Infrastructure.Common.Caching
{
    public sealed class CacheWriteBuffer : ICacheWriteBuffer
    {
        private enum OperationType
        {
            SetString,
            Remove
        }

        private sealed record PendingOperation(
            OperationType Type,
            string Key,
            string? Value = null,
            DateTime? ExpiresAtUtc = null);

        private readonly List<PendingOperation> _pendingOperations = new();
        private readonly ICacheService _cacheService;

        public CacheWriteBuffer(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public void QueueSetString(string key, string value, DateTime expiresAtUtc)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (expiresAtUtc <= SecurityTime.UtcNow)
                return;

            _pendingOperations.Add(new PendingOperation(
                OperationType.SetString,
                key.Trim(),
                value,
                ExpiresAtUtc: expiresAtUtc));
        }

        public void QueueRemove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _pendingOperations.Add(new PendingOperation(
                OperationType.Remove,
                key.Trim()));
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_pendingOperations.Count == 0)
                return;

            var operationsToExecute = _pendingOperations.ToArray();
            _pendingOperations.Clear();

            foreach (var operation in operationsToExecute)
            {
                if (operation.Type == OperationType.SetString)
                {
                    var ttl = (operation.ExpiresAtUtc ?? SecurityTime.UtcNow) - SecurityTime.UtcNow;
                    if (ttl <= TimeSpan.Zero)
                        continue;

                    await _cacheService.SetAsync(
                        operation.Key,
                        operation.Value ?? string.Empty,
                        ttl,
                        cancellationToken);
                    continue;
                }

                await _cacheService.RemoveAsync(operation.Key, cancellationToken);
            }
        }

        public void Clear() => _pendingOperations.Clear();
    }
}
