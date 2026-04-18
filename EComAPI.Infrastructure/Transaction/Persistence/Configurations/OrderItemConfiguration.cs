using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("OrderItems");

            entityTypeBuilder.HasKey(orderItem => orderItem.Id);

            entityTypeBuilder.Property(orderItem => orderItem.OrderId).IsRequired();
            entityTypeBuilder.Property(orderItem => orderItem.ProductVariantId).IsRequired();

            entityTypeBuilder.Property(orderItem => orderItem.SnapshotProductName)
                .IsRequired()
                .HasMaxLength(255);

            entityTypeBuilder.Property(orderItem => orderItem.SnapshotVariantName)
                .IsRequired()
                .HasMaxLength(100);

            entityTypeBuilder.Property(orderItem => orderItem.SnapshotPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entityTypeBuilder.Property(orderItem => orderItem.Quantity).IsRequired();

            entityTypeBuilder.Property(orderItem => orderItem.CreatedAt).IsRequired();
            entityTypeBuilder.Property(orderItem => orderItem.CreatedBy).IsRequired();
            entityTypeBuilder.Property(orderItem => orderItem.UpdatedAt).IsRequired(false);
            entityTypeBuilder.Property(orderItem => orderItem.UpdatedBy).IsRequired(false);
            entityTypeBuilder.Property(orderItem => orderItem.DeletedAt).IsRequired(false);
            entityTypeBuilder.Property(orderItem => orderItem.DeletedBy).IsRequired(false);
        }
    }
}
