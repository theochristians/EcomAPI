using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Transaction.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Transaction.Persistence.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly AppDbContext _appDbContext;

        public CouponRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        // COUPON
        public async Task<Coupon?> GetCouponByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Coupons
                .FirstOrDefaultAsync(coupon => coupon.Id == id, cancellationToken);
        }

        public async Task<Coupon?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Coupons
                .FirstOrDefaultAsync(coupon => coupon.Code == code.ToUpperInvariant(), cancellationToken);
        }

        public async Task<IReadOnlyList<Coupon>> GetAllCouponsAsync(CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Coupons
                .OrderByDescending(coupon => coupon.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddCouponAsync(Coupon coupon, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Coupons.AddAsync(coupon, cancellationToken);
        }

        public Task UpdateCouponAsync(Coupon coupon, CancellationToken cancellationToken = default)
        {
            _appDbContext.Coupons.Update(coupon);
            return Task.CompletedTask;
        }

        public async Task<bool> CouponCodeExistsAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Coupons
                .AnyAsync(coupon => coupon.Code == code.ToUpperInvariant(), cancellationToken);
        }

        // COUPON USAGE
        public async Task AddCouponUsageAsync(CouponUsage couponUsage, CancellationToken cancellationToken = default)
        {
            await _appDbContext.CouponUsages.AddAsync(couponUsage, cancellationToken);
        }

        public async Task<bool> UserHasUsedCouponAsync(Guid userId, Guid couponId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.CouponUsages
                .AnyAsync(couponUsage => couponUsage.UserId == userId && couponUsage.CouponId == couponId, cancellationToken);
        }
    }
}
