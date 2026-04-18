using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    public class Coupon : BaseEntity
    {
        public string Code { get; private set; } = string.Empty;
        public decimal DiscountAmount { get; private set; }
        public string DiscountType { get; private set; } = string.Empty;
        public decimal MinimumPurchase { get; private set; }
        public decimal? MaxDiscount { get; private set; }
        public int MaxUsage { get; private set; }
        public int UsedCount { get; private set; }
        public DateTime ValidFrom { get; private set; }
        public DateTime ValidUntil { get; private set; }
        public bool IsActive { get; private set; }

        private Coupon() { }

        public Coupon(
            string code,
            decimal discountAmount,
            string discountType,
            int maxUsage,
            DateTime validFrom,
            DateTime validUntil,
            Guid createdBy,
            decimal minimumPurchase = 0,
            decimal? maxDiscount = null)
        {
            var codeValue = Guard.AgainstNullOrWhiteSpace(code, "Code is required");
            Guard.AgainstMaxLength(codeValue, 50, "Code cannot exceed 50 characters");

            Guard.AgainstNegative(discountAmount, "DiscountAmount cannot be negative");

            var discountTypeNormalized = Guard.AgainstNullOrWhiteSpace(discountType, "DiscountType is required").ToLowerInvariant();
            if (discountTypeNormalized != CouponDiscountType.Fixed && discountTypeNormalized != CouponDiscountType.Percentage)
                throw new DomainException("DiscountType must be 'fixed' or 'percentage'");

            Guard.AgainstNegative(minimumPurchase, "MinimumPurchase cannot be negative");
            Guard.AgainstNegative(maxUsage, "MaxUsage cannot be negative");

            if (maxDiscount.HasValue)
                Guard.AgainstNegative(maxDiscount.Value, "MaxDiscount cannot be negative");

            if (validUntil <= validFrom)
                throw new DomainException("ValidUntil must be after ValidFrom");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            Code = codeValue.ToUpperInvariant();
            DiscountAmount = discountAmount;
            DiscountType = discountTypeNormalized;
            MinimumPurchase = minimumPurchase;
            MaxDiscount = maxDiscount;
            MaxUsage = maxUsage;
            UsedCount = 0;
            ValidFrom = validFrom;
            ValidUntil = validUntil;
            IsActive = true;

            SetCreated(createdBy);
        }

        // ==========================================
        // BUSINESS LOGIC
        // ==========================================

        public bool IsValid(DateTime checkDate)
        {
            return IsActive
                && UsedCount < MaxUsage
                && checkDate >= ValidFrom
                && checkDate <= ValidUntil;
        }

        public decimal CalculateDiscount(decimal totalAmount)
        {
            if (totalAmount < MinimumPurchase)
                return 0;

            if (DiscountType == CouponDiscountType.Percentage)
            {
                var discount = totalAmount * (DiscountAmount / 100);
                return MaxDiscount.HasValue ? Math.Min(discount, MaxDiscount.Value) : discount;
            }

            return Math.Min(DiscountAmount, totalAmount);
        }

        public void Use(Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            if (UsedCount >= MaxUsage)
                throw new DomainException("Coupon has reached its maximum usage limit");

            UsedCount++;
            SetUpdated(updatedBy);
        }

        public void Deactivate(Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");
            IsActive = false;
            SetUpdated(updatedBy);
        }

        public void Activate(Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");
            IsActive = true;
            SetUpdated(updatedBy);
        }
    }
}
