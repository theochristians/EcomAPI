namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record CreateCouponRequest(
        string Code,
        decimal DiscountAmount,
        string DiscountType,
        int MaxUsage,
        DateTime ValidFrom,
        DateTime ValidUntil,
        decimal MinimumPurchase = 0,
        decimal? MaxDiscount = null);
}
