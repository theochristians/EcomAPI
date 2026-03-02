using System.Text.RegularExpressions;
using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.ValueObjects
{
    public sealed class EmailAddress : ValueObject
    {
        private static readonly Regex EmailRegex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string Value { get; }

        private EmailAddress(string value)
        {
            Value = value;
        }

        public static EmailAddress Create(string email)
        {
            var value = Guard.AgainstNullOrWhiteSpace(email, "Email address cannot be empty").ToLowerInvariant();
            Guard.AgainstMaxLength(value, 255, "Email address cannot exceed 255 characters");

            if (!EmailRegex.IsMatch(value))
                throw new DomainException($"The email address '{value}' is not valid.");

            return new EmailAddress(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(EmailAddress email)
        {
            Guard.AgainstNull(email, "Email address is required");
            return email.Value;
        }
    }
}
