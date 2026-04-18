namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record CouponResponse(
        Guid Id,
        string Code,
        decimal DiscountAmount,
        string DiscountType,
        decimal MinimumPurchase,
        decimal? MaxDiscount,
        int MaxUsage,
        int UsedCount,
        DateTime ValidFrom,
        DateTime ValidUntil,
        bool IsActive);
}
