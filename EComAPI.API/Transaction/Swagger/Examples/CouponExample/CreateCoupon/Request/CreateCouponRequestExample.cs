using EComAPI.API.Transaction.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Transaction.Swagger.Examples.CouponExample.CreateCoupon.Request
{
    public class CreateCouponRequestExample : IExamplesProvider<CreateCouponRequest>
    {
        public CreateCouponRequest GetExamples()
            => new CreateCouponRequest(
                Code: "DISKON20",
                DiscountAmount: 20,
                DiscountType: "percentage",
                MaxUsage: 100,
                ValidFrom: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ValidUntil: new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                MinimumPurchase: 100000,
                MaxDiscount: 50000);
    }
}
