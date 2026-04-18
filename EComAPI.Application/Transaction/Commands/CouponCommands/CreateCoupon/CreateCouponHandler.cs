using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Commands.CouponCommands.CreateCoupon
{
    public class CreateCouponHandler
    {
        private readonly ICouponRepository _couponRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCouponHandler(
            ICouponRepository couponRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _couponRepository = couponRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateCouponCommand createCouponCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                var codeAlreadyExists = await _couponRepository.CouponCodeExistsAsync(
                    createCouponCommand.Code, cancellationToken);

                if (codeAlreadyExists)
                    return Result<Guid>.Failure($"Coupon code '{createCouponCommand.Code}' already exists");

                var coupon = new Coupon(
                    code: createCouponCommand.Code,
                    discountAmount: createCouponCommand.DiscountAmount,
                    discountType: createCouponCommand.DiscountType,
                    maxUsage: createCouponCommand.MaxUsage,
                    validFrom: createCouponCommand.ValidFrom,
                    validUntil: createCouponCommand.ValidUntil,
                    createdBy: _currentUser.UserId,
                    minimumPurchase: createCouponCommand.MinimumPurchase,
                    maxDiscount: createCouponCommand.MaxDiscount);

                await _couponRepository.AddCouponAsync(coupon, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(coupon.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<Guid>.Failure("An error occurred while creating the coupon");
            }
        }
    }
}
