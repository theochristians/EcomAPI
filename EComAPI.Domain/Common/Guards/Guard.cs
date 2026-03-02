using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Domain.Common.Guards
{
    public static class Guard
    {
        public static void AgainstEmptyGuid(Guid value, string message)
        {
            if (value == Guid.Empty)
                throw new DomainException(message);
        }

        public static void AgainstNull(object? value, string message)
        {
            if (value is null)
                throw new DomainException(message);
        }

        public static string AgainstNullOrWhiteSpace(string value, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException(message);

            return value.Trim();
        }

        public static void AgainstMinLength(string value, int minLength, string message)
        {
            if (value.Length < minLength)
                throw new DomainException(message);
        }

        public static void AgainstMaxLength(string value, int maxLength, string message)
        {
            if (value.Length > maxLength)
                throw new DomainException(message);
        }

        public static string? AgainstLengthIfProvided(string? value, int min, int max, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();

            if (trimmed.Length < min || trimmed.Length > max)
                throw new DomainException(message);

            return trimmed;
        }

        public static string? AgainstMaxLengthIfProvided(string? value, int max, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();

            if (trimmed.Length > max)
                throw new DomainException(message);

            return trimmed;
        }

        public static int AgainstNegative(int value, string message)
        {
            if (value < 0)
                throw new DomainException(message);

            return value;
        }

        public static decimal AgainstNegative(decimal value, string message)
        {
            if (value < 0)
                throw new DomainException(message);

            return value;
        }

        public static int? AgainstNegativeIfProvided(int? value, string message)
        {
            if (!value.HasValue)
                return null;

            if (value.Value < 0)
                throw new DomainException(message);

            return value.Value;
        }

        public static decimal? AgainstNegativeIfProvided(decimal? value, string message)
        {
            if (!value.HasValue)
                return null;

            if (value.Value < 0)
                throw new DomainException(message);

            return value.Value;
        }
    }
}
