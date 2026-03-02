using System.Text.Json.Serialization;

namespace EComAPI.API.Tests.Infrastructure
{
    /// <summary>
    /// Plain deserialization wrapper for ApiResponse&lt;T&gt; in integration tests.
    /// The real ApiResponse&lt;T&gt; has a protected constructor and cannot be deserialized
    /// by System.Text.Json, so we use this mirror class instead.
    /// </summary>
    public class TestApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}
