using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Coupons");

            entityTypeBuilder.HasKey(coupon => coupon.Id);

            entityTypeBuilder.Property(coupon => coupon.Code)
                .IsRequired()
                .HasMaxLength(50);
            entityTypeBuilder.HasIndex(coupon => coupon.Code).IsUnique();

            entityTypeBuilder.Property(coupon => coupon.DiscountAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entityTypeBuilder.Property(coupon => coupon.DiscountType)
                .IsRequired()
                .HasMaxLength(20);

            entityTypeBuilder.Property(coupon => coupon.MinimumPurchase)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entityTypeBuilder.Property(coupon => coupon.MaxDiscount)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            entityTypeBuilder.Property(coupon => coupon.MaxUsage).IsRequired();
            entityTypeBuilder.Property(coupon => coupon.UsedCount).IsRequired().HasDefaultValue(0);

            entityTypeBuilder.Property(coupon => coupon.ValidFrom).IsRequired();
            entityTypeBuilder.Property(coupon => coupon.ValidUntil).IsRequired();

            entityTypeBuilder.Property(coupon => coupon.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entityTypeBuilder.Property(coupon => coupon.CreatedAt).IsRequired();
            entityTypeBuilder.Property(coupon => coupon.CreatedBy).IsRequired();
            entityTypeBuilder.Property(coupon => coupon.UpdatedAt).IsRequired(false);
            entityTypeBuilder.Property(coupon => coupon.UpdatedBy).IsRequired(false);
            entityTypeBuilder.Property(coupon => coupon.DeletedAt).IsRequired(false);
            entityTypeBuilder.Property(coupon => coupon.DeletedBy).IsRequired(false);
        }
    }
}
