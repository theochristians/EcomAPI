using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
    {
        public void Configure(EntityTypeBuilder<CouponUsage> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("CouponUsages");

            entityTypeBuilder.HasKey(couponUsage => couponUsage.Id);

            entityTypeBuilder.Property(couponUsage => couponUsage.CouponId).IsRequired();
            entityTypeBuilder.Property(couponUsage => couponUsage.UserId).IsRequired();
            entityTypeBuilder.Property(couponUsage => couponUsage.OrderId).IsRequired();

            entityTypeBuilder.Property(couponUsage => couponUsage.UsedAt).IsRequired();
            entityTypeBuilder.Property(couponUsage => couponUsage.CreatedAt).IsRequired();

            entityTypeBuilder.HasIndex(couponUsage => new { couponUsage.CouponId, couponUsage.UserId });
            entityTypeBuilder.HasIndex(couponUsage => couponUsage.OrderId);
        }
    }
}
