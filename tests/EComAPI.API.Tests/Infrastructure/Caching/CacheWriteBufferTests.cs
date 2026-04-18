using EComAPI.Application.Common.Interfaces;
using EComAPI.Infrastructure.Common.Caching;
using FluentAssertions;
using Xunit;

namespace EComAPI.API.Tests.Infrastructure.Caching
{
    public class CacheWriteBufferTests
    {
        [Fact]
        public async Task FlushAsync_QueuedSetString_ShouldWriteToCacheService()
        {
            var cacheService = new FakeCacheService();
            var buffer = new CacheWriteBuffer(cacheService);

            buffer.QueueSetString("auth:blacklist:token", "1", DateTime.UtcNow.AddMinutes(5));
            await buffer.FlushAsync();

            cacheService.SetCalls.Should().ContainSingle();
            cacheService.SetCalls[0].Key.Should().Be("auth:blacklist:token");
            cacheService.SetCalls[0].Value.Should().Be("1");
        }

        [Fact]
        public async Task FlushAsync_QueuedRemove_ShouldCallRemoveAsync()
        {
            var cacheService = new FakeCacheService();
            var buffer = new CacheWriteBuffer(cacheService);

            buffer.QueueRemove("auth:blacklist:token");
            await buffer.FlushAsync();

            cacheService.RemoveCalls.Should().ContainSingle();
            cacheService.RemoveCalls[0].Should().Be("auth:blacklist:token");
        }

        [Fact]
        public async Task Clear_ShouldDropQueuedOperations()
        {
            var cacheService = new FakeCacheService();
            var buffer = new CacheWriteBuffer(cacheService);

            buffer.QueueSetString("key-1", "value-1", DateTime.UtcNow.AddMinutes(1));
            buffer.QueueRemove("key-2");
            buffer.Clear();
            await buffer.FlushAsync();

            cacheService.SetCalls.Should().BeEmpty();
            cacheService.RemoveCalls.Should().BeEmpty();
        }

        private sealed class FakeCacheService : ICacheService
        {
            public List<(string Key, string Value, TimeSpan Ttl)> SetCalls { get; } = new();
            public List<string> RemoveCalls { get; } = new();

            public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
                => Task.FromResult(default(T));

            public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
            {
                var valueAsString = value?.ToString() ?? string.Empty;
                SetCalls.Add((key, valueAsString, ttl));
                return Task.CompletedTask;
            }

            public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
            {
                RemoveCalls.Add(key);
                return Task.CompletedTask;
            }

            public Task<long> GetNamespaceVersionAsync(string namespaceName, CancellationToken cancellationToken = default)
                => Task.FromResult(1L);

            public Task<IReadOnlyDictionary<string, long>> GetNamespaceVersionsAsync(
                IEnumerable<string> namespaceNames,
                CancellationToken cancellationToken = default)
            {
                var versions = namespaceNames.Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(namespaceName => namespaceName, _ => 1L, StringComparer.OrdinalIgnoreCase);

                return Task.FromResult<IReadOnlyDictionary<string, long>>(versions);
            }

            public Task<long> IncrementNamespaceVersionAsync(string namespaceName, CancellationToken cancellationToken = default)
                => Task.FromResult(1L);
        }
    }
}
