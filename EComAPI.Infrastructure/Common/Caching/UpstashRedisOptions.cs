namespace EComAPI.Infrastructure.Common.Caching
{
    public sealed class UpstashRedisOptions
    {
        public bool Enabled { get; init; }
        public string Url { get; init; } = string.Empty;
        public string Token { get; init; } = string.Empty;
        public string InstanceName { get; init; } = "ecomapi:";
    }
}
