using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using System.Globalization;

namespace EComAPI.Domain.Products.ValueObjects
{
    public sealed class Money : ValueObject
    {
        public decimal Amount { get; }

        private Money(decimal amount)
        {
            if (amount < 0)
                throw new DomainException("Amount cannot be negative");

            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        public static Money FromDecimal(decimal amount) => new(amount);

        public static Money Zero() => new(0m);

        public Money Add(Money other)
        {
            GuardAgainstNull(other, "Other money is required");
            return new(Amount + other.Amount);
        }

        public Money Subtract(Money other)
        {
            GuardAgainstNull(other, "Other money is required");

            if (Amount < other.Amount)
                throw new DomainException("Cannot subtract a larger amount from a smaller amount");

            return new(Amount - other.Amount);
        }

        public Money Multiply(decimal multiplier)
        {
            if (multiplier < 0)
                throw new DomainException("Multiplier cannot be negative");

            return new(Amount * multiplier);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
        }

        public override string ToString() => Amount.ToString("0.00", CultureInfo.InvariantCulture);

        public static implicit operator decimal(Money money)
        {
            GuardAgainstNull(money, "Money is required");
            return money.Amount;
        }

        public static bool operator >(Money left, Money right)
        {
            GuardAgainstNull(left, "Left money is required");
            GuardAgainstNull(right, "Right money is required");
            return left.Amount > right.Amount;
        }

        public static bool operator <(Money left, Money right)
        {
            GuardAgainstNull(left, "Left money is required");
            GuardAgainstNull(right, "Right money is required");
            return left.Amount < right.Amount;
        }

        public static bool operator >=(Money left, Money right)
        {
            GuardAgainstNull(left, "Left money is required");
            GuardAgainstNull(right, "Right money is required");
            return left.Amount >= right.Amount;
        }

        public static bool operator <=(Money left, Money right)
        {
            GuardAgainstNull(left, "Left money is required");
            GuardAgainstNull(right, "Right money is required");
            return left.Amount <= right.Amount;
        }

        private static void GuardAgainstNull(object? value, string message)
        {
            if (value is null)
                throw new DomainException(message);
        }
    }
}
