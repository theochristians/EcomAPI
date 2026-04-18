using EComAPI.Domain.Transaction.Enums;

namespace EComAPI.Application.Common.Constants
{
    public static class ShippingCosts
    {
        public const decimal Regular = 15_000;
        public const decimal Express = 50_000;
        public const decimal SameDay = 100_000;

        public static decimal GetCost(ShippingType shippingType) => shippingType switch
        {
            ShippingType.Regular => Regular,
            ShippingType.Express => Express,
            ShippingType.SameDay => SameDay,
            _ => Regular
        };
    }
}
