using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Auth.ValueObjects
{
    public sealed class PasswordHash : ValueObject
    {
        public string Value { get; }

        private PasswordHash(string value)
        {
            Value = value;
        }

        public static PasswordHash FromHash(string hash)
        {
            var value = Guard.AgainstNullOrWhiteSpace(hash, "Password hash cannot be empty");
            Guard.AgainstMaxLength(value, 500, "Password hash cannot exceed 500 characters");
            return new PasswordHash(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(PasswordHash passwordHash)
        {
            Guard.AgainstNull(passwordHash, "Password hash is required");
            return passwordHash.Value;
        }
    }
}
