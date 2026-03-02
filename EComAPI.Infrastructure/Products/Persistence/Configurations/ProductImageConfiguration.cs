using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EComAPI.Domain.Products.Entities; 

namespace EComAPI.Infrastructure.Products.Persistence.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("ProductImages");

            entityTypeBuilder.HasKey(productImage => productImage.Id);

            entityTypeBuilder.Property(productImage => productImage.ProductId)
                .IsRequired();

            entityTypeBuilder.Property(productImage => productImage.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            entityTypeBuilder.Property(productImage => productImage.IsPrimary)
                .HasDefaultValue(false)
                .IsRequired();

            entityTypeBuilder.Property(productImage => productImage.DisplayOrder)
                .IsRequired();

            entityTypeBuilder.HasOne(productImage => productImage.Product)
                .WithMany(product => product.Images)
                .HasForeignKey(productImage => productImage.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(productImage => productImage.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(productImage => productImage.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(productImage => productImage.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(productImage => productImage.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(productImage => productImage.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(productImage => productImage.DeletedBy)
                .IsRequired(false);
        }
    }
}