using EComAPI.Application.Common.Interfaces;
using EComAPI.Infrastructure.Common.Caching;
using FluentAssertions;
using Xunit;

namespace EComAPI.API.Tests.Infrastructure.Caching
{
    public class CacheInvalidationBufferTests
    {
        [Fact]
        public async Task FlushAsync_SameNamespaceMarkedMultipleTimes_ShouldIncrementOnce()
        {
            var cacheService = new FakeCacheService();
            var buffer = new CacheInvalidationBuffer(cacheService);

            buffer.MarkNamespaceDirty("products");
            buffer.MarkNamespaceDirty("products");
            buffer.MarkNamespaceDirty("products");

            await buffer.FlushAsync();

            cacheService.IncrementCalls.Should().ContainSingle();
            cacheService.IncrementCalls.Should().Contain("products");
        }

        [Fact]
        public async Task FlushAsync_DifferentNamespaces_ShouldIncrementEachNamespace()
        {
            var cacheService = new FakeCacheService();
            var buffer = new CacheInvalidationBuffer(cacheService);

            buffer.MarkNamespaceDirty("products");
            buffer.MarkNamespaceDirty("categories");

            await buffer.FlushAsync();

            cacheService.IncrementCalls.Should().BeEquivalentTo(new[] { "products", "categories" });
        }

        [Fact]
        public async Task Clear_BeforeFlush_ShouldDropPendingInvalidations()
        {
            var cacheService = new FakeCacheService();
            var buffer = new CacheInvalidationBuffer(cacheService);

            buffer.MarkNamespaceDirty("products");
            buffer.Clear();

            await buffer.FlushAsync();

            cacheService.IncrementCalls.Should().BeEmpty();
        }

        private sealed class FakeCacheService : ICacheService
        {
            public List<string> IncrementCalls { get; } = new();

            public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
                => Task.FromResult(default(T));

            public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

            public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
                => Task.CompletedTask;

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
            {
                IncrementCalls.Add(namespaceName);
                return Task.FromResult(1L);
            }
        }
    }
}
