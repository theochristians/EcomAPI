using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EComAPI.Domain.Products.Entities; 

namespace EComAPI.Infrastructure.Products.Persistence.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("ProductVariants");

            entityTypeBuilder.HasKey(productVariant => productVariant.Id);

            entityTypeBuilder.Property(productVariant => productVariant.ProductId)
                .IsRequired();

            entityTypeBuilder.Property(productVariant => productVariant.Sku)
                .IsRequired()
                .HasMaxLength(50);

            entityTypeBuilder.HasIndex(productVariant => productVariant.Sku)
                .IsUnique();

            entityTypeBuilder.Property(productVariant => productVariant.Size)
                .IsRequired(false);

            entityTypeBuilder.Property(productVariant => productVariant.Color)
                .IsRequired(false);

            entityTypeBuilder.Property(productVariant => productVariant.PriceAdjustment)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            entityTypeBuilder.Property(productVariant => productVariant.Stock)
                .HasDefaultValue(0)
                .IsRequired()
                .IsConcurrencyToken();

            entityTypeBuilder.Property(productVariant => productVariant.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entityTypeBuilder.HasOne(productVariant => productVariant.Product)
                .WithMany(product => product.Variants)
                .HasForeignKey(productVariant => productVariant.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(productVariant => productVariant.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(productVariant => productVariant.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(productVariant => productVariant.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(productVariant => productVariant.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(productVariant => productVariant.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(productVariant => productVariant.DeletedBy)
                .IsRequired(false);
        }
    }
}
