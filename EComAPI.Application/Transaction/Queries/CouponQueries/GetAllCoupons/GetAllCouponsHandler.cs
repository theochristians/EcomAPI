using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Queries.CouponQueries.GetAllCoupons
{
    public class GetAllCouponsHandler
    {
        private readonly ICouponRepository _couponRepository;
        private readonly ICurrentUser _currentUser;

        public GetAllCouponsHandler(
            ICouponRepository couponRepository,
            ICurrentUser currentUser)
        {
            _couponRepository = couponRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<CouponDto>>> Handle(
            GetAllCouponsQuery getAllCouponsQuery,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<IReadOnlyList<CouponDto>>.Failure("User not authenticated");

                var coupons = await _couponRepository.GetAllCouponsAsync(cancellationToken);

                var couponDtos = coupons
                    .Select(coupon => new CouponDto(
                        coupon.Id,
                        coupon.Code,
                        coupon.DiscountAmount,
                        coupon.DiscountType,
                        coupon.MinimumPurchase,
                        coupon.MaxDiscount,
                        coupon.MaxUsage,
                        coupon.UsedCount,
                        coupon.ValidFrom,
                        coupon.ValidUntil,
                        coupon.IsActive))
                    .ToList()
                    .AsReadOnly();

                return Result<IReadOnlyList<CouponDto>>.Success(couponDtos);
            }
            catch (DomainException domainException)
            {
                return Result<IReadOnlyList<CouponDto>>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<IReadOnlyList<CouponDto>>.Failure("An error occurred while retrieving coupons");
            }
        }
    }
}
