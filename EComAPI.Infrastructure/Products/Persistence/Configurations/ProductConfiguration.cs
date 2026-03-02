using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EComAPI.Domain.Products.Entities;

namespace EComAPI.Infrastructure.Products.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Products");

            entityTypeBuilder.HasKey(product => product.Id);

            entityTypeBuilder.Property(product => product.CategoryId)
                .IsRequired();

            entityTypeBuilder.Property(product => product.Name)
                .IsRequired()
                .HasMaxLength(255);

            entityTypeBuilder.Property(product => product.Slug)
                .IsRequired()
                .HasMaxLength(300);

            entityTypeBuilder.HasIndex(product => product.Slug)
                .IsUnique();

            entityTypeBuilder.Property(product => product.Description)
                .IsRequired(false);

            entityTypeBuilder.Property(product => product.BasePrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entityTypeBuilder.Property(product => product.ViewCount)
                .HasDefaultValue(0)
                .IsRequired();

            entityTypeBuilder.Property(product => product.TotalStock)
                .IsRequired()
                .HasDefaultValue(0);

            entityTypeBuilder.Property(product => product.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entityTypeBuilder.HasOne<Domain.Categories.Entities.Category>()
                .WithMany()
                .HasForeignKey(product => product.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entityTypeBuilder.HasMany(product => product.Variants)
                .WithOne(productVariant => productVariant.Product)
                .HasForeignKey(productVariant => productVariant.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.HasMany(product => product.Images)
                .WithOne(productImage => productImage.Product)
                .HasForeignKey(productImage => productImage.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(product => product.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(product => product.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(product => product.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(product => product.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(product => product.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(product => product.DeletedBy)
                .IsRequired(false);
        }
    }
}