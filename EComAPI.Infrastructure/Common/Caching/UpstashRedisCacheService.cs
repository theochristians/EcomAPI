using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace EComAPI.Infrastructure.Common.Caching
{
    public sealed class UpstashRedisCacheService : ICacheService
    {
        private readonly HttpClient _httpClient;
        private readonly UpstashRedisOptions _options;
        private readonly ILogger<UpstashRedisCacheService> _logger;
        private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

        private sealed class UpstashCommandResponse
        {
            public JsonElement? Result { get; set; }
            public string? Error { get; set; }
        }

        public UpstashRedisCacheService(
            HttpClient httpClient,
            UpstashRedisOptions options,
            ILogger<UpstashRedisCacheService> logger)
        {
            _httpClient = httpClient;
            _options = options;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteCommandAsync(new object[] { "GET", ResolveKey(key) }, cancellationToken);

            if (result is null || result.Value.ValueKind == JsonValueKind.Null)
                return default;

            if (typeof(T) == typeof(string))
            {
                var stringValue = result.Value.ValueKind == JsonValueKind.String
                    ? result.Value.GetString()
                    : result.Value.GetRawText();

                return (T?)(object?)stringValue;
            }

            var rawValue = result.Value.ValueKind == JsonValueKind.String
                ? result.Value.GetString()
                : result.Value.GetRawText();

            if (string.IsNullOrWhiteSpace(rawValue))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(rawValue, _serializerOptions);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to deserialize cache key {CacheKey}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
        {
            if (ttl <= TimeSpan.Zero)
                return;

            var payload = value is string stringValue
                ? stringValue
                : JsonSerializer.Serialize(value, _serializerOptions);

            var expirySeconds = Math.Max(1, (long)Math.Ceiling(ttl.TotalSeconds));

            _ = await ExecuteCommandAsync(new object[] { "SET", ResolveKey(key), payload, "EX", expirySeconds }, cancellationToken);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _ = await ExecuteCommandAsync(new object[] { "DEL", ResolveKey(key) }, cancellationToken);
        }

        public async Task<long> GetNamespaceVersionAsync(string namespaceName, CancellationToken cancellationToken = default)
        {
            var versionKey = CacheKeys.NamespaceVersion(namespaceName);
            var existingVersion = await GetAsync<string>(versionKey, cancellationToken);

            if (long.TryParse(existingVersion, out var version) && version > 0)
                return version;

            await SetAsync(versionKey, "1", TimeSpan.FromDays(3650), cancellationToken);
            return 1;
        }

        public async Task<IReadOnlyDictionary<string, long>> GetNamespaceVersionsAsync(
            IEnumerable<string> namespaceNames,
            CancellationToken cancellationToken = default)
        {
            var distinctNamespaceNames = namespaceNames
                .Where(namespaceName => !string.IsNullOrWhiteSpace(namespaceName))
                .Select(namespaceName => namespaceName.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var versions = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            if (distinctNamespaceNames.Length == 0)
                return versions;

            var versionKeys = distinctNamespaceNames
                .Select(CacheKeys.NamespaceVersion)
                .ToArray();

            var command = new object[versionKeys.Length + 1];
            command[0] = "MGET";

            for (var i = 0; i < versionKeys.Length; i++)
            {
                command[i + 1] = ResolveKey(versionKeys[i]);
            }

            var result = await ExecuteCommandAsync(command, cancellationToken);
            if (result is not null && result.Value.ValueKind == JsonValueKind.Array)
            {
                var values = result.Value;
                var length = values.GetArrayLength();

                for (var i = 0; i < distinctNamespaceNames.Length; i++)
                {
                    var namespaceName = distinctNamespaceNames[i];

                    if (i < length && TryParsePositiveLong(values[i], out var version))
                    {
                        versions[namespaceName] = version;
                        continue;
                    }

                    versions[namespaceName] = 1;
                    await SetAsync(versionKeys[i], "1", TimeSpan.FromDays(3650), cancellationToken);
                }

                return versions;
            }

            foreach (var namespaceName in distinctNamespaceNames)
            {
                versions[namespaceName] = await GetNamespaceVersionAsync(namespaceName, cancellationToken);
            }

            return versions;
        }

        public async Task<long> IncrementNamespaceVersionAsync(string namespaceName, CancellationToken cancellationToken = default)
        {
            var versionKey = CacheKeys.NamespaceVersion(namespaceName);
            var result = await ExecuteCommandAsync(new object[] { "INCR", ResolveKey(versionKey) }, cancellationToken);

            if (result is not null && result.Value.ValueKind == JsonValueKind.Number && result.Value.TryGetInt64(out var version) && version > 0)
                return version;

            return await GetNamespaceVersionAsync(namespaceName, cancellationToken);
        }

        private async Task<JsonElement?> ExecuteCommandAsync(object[] command, CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
                return null;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, string.Empty)
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(command, _serializerOptions),
                        Encoding.UTF8,
                        "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Redis command failed with HTTP {StatusCode}. Body: {ResponseBody}", (int)response.StatusCode, body);
                    return null;
                }

                var commandResponse = JsonSerializer.Deserialize<UpstashCommandResponse>(body, _serializerOptions);

                if (!string.IsNullOrWhiteSpace(commandResponse?.Error))
                {
                    _logger.LogWarning("Redis command returned error: {RedisError}", commandResponse.Error);
                    return null;
                }

                return commandResponse?.Result;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Redis command execution failed");
                return null;
            }
        }

        private string ResolveKey(string key)
            => $"{_options.InstanceName}{key}";

        private static bool TryParsePositiveLong(JsonElement value, out long result)
        {
            result = 0;

            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.TryGetInt64(out result) && result > 0;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                return long.TryParse(value.GetString(), out result) && result > 0;
            }

            return false;
        }
    }
}

