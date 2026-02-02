using EComAPI.Domain.Product.Entities;
using EComAPI.Infrastructure.Common.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Products.Persistence.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Sku)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(v => v.Sku).IsUnique();

            builder.Property(v => v.Stock)
                .HasDefaultValue(0);

            builder.Property(v => v.PriceAdjustment)
                .HasColumnType("decimal(18,2)");

            builder.Property(v => v.IsActive)
                .HasDefaultValue(true);

            builder.ConfigureAudit();
        }
    }
}