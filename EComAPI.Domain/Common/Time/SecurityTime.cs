namespace EComAPI.Domain.Common.Time
{
    public static class SecurityTime
    {
        public static DateTime UtcNow => DateTime.UtcNow;

        public static DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
    }
}