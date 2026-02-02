using System.Collections.Generic;
using System.Text.RegularExpressions;
using EComAPI.Domain.Auth.Exceptions;
using EComAPI.Domain.Common.Base;

namespace EComAPI.Domain.Auth.ValueObjects
{
    public sealed class EmailAddress : ValueObject
    {
        public string Value { get; }

        private EmailAddress(string value)
        {
            Value = value;
        }

        public static EmailAddress Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidEmailAddressException(email);

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new InvalidEmailAddressException(email);

            return new EmailAddress(email.Trim().ToLowerInvariant());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}