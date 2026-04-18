using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Interfaces
{
    public interface ICouponRepository
    {
        // COUPON
        Task<Coupon?> GetCouponByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Coupon?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Coupon>> GetAllCouponsAsync(CancellationToken cancellationToken = default);
        Task AddCouponAsync(Coupon coupon, CancellationToken cancellationToken = default);
        Task UpdateCouponAsync(Coupon coupon, CancellationToken cancellationToken = default);
        Task<bool> CouponCodeExistsAsync(string code, CancellationToken cancellationToken = default);

        // COUPON USAGE
        Task AddCouponUsageAsync(CouponUsage couponUsage, CancellationToken cancellationToken = default);
        Task<bool> UserHasUsedCouponAsync(Guid userId, Guid couponId, CancellationToken cancellationToken = default);
    }
}
