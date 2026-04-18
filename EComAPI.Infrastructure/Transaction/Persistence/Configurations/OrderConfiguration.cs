using EComAPI.Domain.Transaction.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Orders");

            entityTypeBuilder.HasKey(order => order.Id);

            entityTypeBuilder.Property(order => order.UserId).IsRequired();
            entityTypeBuilder.Property(order => order.AddressId).IsRequired(false);

            entityTypeBuilder.Property(order => order.OrderNumber)
                .IsRequired()
                .HasMaxLength(50);
            entityTypeBuilder.HasIndex(order => order.OrderNumber).IsUnique();

            entityTypeBuilder.Property(order => order.ShippingRecipientName)
                .IsRequired()
                .HasMaxLength(255);

            entityTypeBuilder.Property(order => order.ShippingPhone)
                .IsRequired()
                .HasMaxLength(20);

            entityTypeBuilder.Property(order => order.ShippingFullAddress)
                .IsRequired();

            entityTypeBuilder.Property(order => order.ShippingCity)
                .IsRequired()
                .HasMaxLength(100);

            entityTypeBuilder.Property(order => order.ShippingPostalCode)
                .IsRequired()
                .HasMaxLength(10);

            entityTypeBuilder.Property(order => order.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entityTypeBuilder.Property(order => order.ShippingCost)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entityTypeBuilder.Property(order => order.CouponId).IsRequired(false);

            entityTypeBuilder.Property(order => order.DiscountAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entityTypeBuilder.Property(order => order.FinalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entityTypeBuilder.Property(order => order.Status)
                .IsRequired()
                .HasMaxLength(50);

            entityTypeBuilder.Property(order => order.Courier)
                .HasMaxLength(100)
                .IsRequired(false);

            entityTypeBuilder.Property(order => order.TrackingNumber)
                .HasMaxLength(100)
                .IsRequired(false);

            entityTypeBuilder.Property(order => order.CustomerNote)
                .HasMaxLength(500)
                .IsRequired(false);

            entityTypeBuilder.Property(order => order.AdminNote)
                .HasMaxLength(500)
                .IsRequired(false);

            entityTypeBuilder.HasMany(order => order.Items)
                .WithOne()
                .HasForeignKey(orderItem => orderItem.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(order => order.CreatedAt).IsRequired();
            entityTypeBuilder.Property(order => order.CreatedBy).IsRequired();
            entityTypeBuilder.Property(order => order.UpdatedAt).IsRequired(false);
            entityTypeBuilder.Property(order => order.UpdatedBy).IsRequired(false);
            entityTypeBuilder.Property(order => order.DeletedAt).IsRequired(false);
            entityTypeBuilder.Property(order => order.DeletedBy).IsRequired(false);
        }
    }
}
