using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class ReturnItemConfiguration : IEntityTypeConfiguration<ReturnItem>
    {
        public void Configure(EntityTypeBuilder<ReturnItem> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("ReturnItems");

            entityTypeBuilder.HasKey(returnItem => returnItem.Id);

            entityTypeBuilder.Property(returnItem => returnItem.ReturnId).IsRequired();
            entityTypeBuilder.Property(returnItem => returnItem.OrderItemId).IsRequired();
            entityTypeBuilder.Property(returnItem => returnItem.Quantity).IsRequired();

            entityTypeBuilder.Property(returnItem => returnItem.Condition)
                .HasMaxLength(50)
                .IsRequired(false);

            entityTypeBuilder.Property(returnItem => returnItem.AdminNote)
                .HasMaxLength(500)
                .IsRequired(false);

            entityTypeBuilder.HasIndex(returnItem => returnItem.ReturnId);
            entityTypeBuilder.HasIndex(returnItem => returnItem.OrderItemId);

            entityTypeBuilder.Property(returnItem => returnItem.CreatedAt).IsRequired();
            entityTypeBuilder.Property(returnItem => returnItem.CreatedBy).IsRequired();
            entityTypeBuilder.Property(returnItem => returnItem.UpdatedAt).IsRequired(false);
            entityTypeBuilder.Property(returnItem => returnItem.UpdatedBy).IsRequired(false);
            entityTypeBuilder.Property(returnItem => returnItem.DeletedAt).IsRequired(false);
            entityTypeBuilder.Property(returnItem => returnItem.DeletedBy).IsRequired(false);
        }
    }
}
