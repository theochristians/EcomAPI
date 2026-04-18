namespace EComAPI.Application.Transaction.Commands.CouponCommands.CreateCoupon
{
    public record CreateCouponCommand(
        string Code,
        decimal DiscountAmount,
        string DiscountType,
        int MaxUsage,
        DateTime ValidFrom,
        DateTime ValidUntil,
        decimal MinimumPurchase = 0,
        decimal? MaxDiscount = null);
}
