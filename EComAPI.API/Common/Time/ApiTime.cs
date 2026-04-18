namespace EComAPI.API.Common.Time
{
    public static class ApiTime
    {
        private static readonly Lazy<TimeZoneInfo> TimeZone = new(ResolveTimeZone);

        public static DateTimeOffset ToJakartaOffset(DateTime dateTime)
        {
            var utcDateTime = dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            };

            var jakartaDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZone.Value);
            return new DateTimeOffset(jakartaDateTime, TimeZone.Value.GetUtcOffset(jakartaDateTime));
        }

        private static TimeZoneInfo ResolveTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
        }
    }
}
