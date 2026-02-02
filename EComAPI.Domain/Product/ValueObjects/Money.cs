using EComAPI.Domain.Common.Base;

namespace EComAPI.Domain.Product.ValueObjects
{
    public class Money : ValueObject
    {
        public decimal Amount { get; }

        private Money(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative");

            Amount = amount;
        }

        public static Money FromDecimal(decimal amount)
            => new(amount);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
        }
    }
}