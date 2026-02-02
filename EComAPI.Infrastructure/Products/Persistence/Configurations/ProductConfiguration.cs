using EComAPI.Domain.Categories.Entities;
using EComAPI.Domain.Product.Entities;
using EComAPI.Infrastructure.Common.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Products.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.Slug)
                .IsRequired()
                .HasMaxLength(300);

            builder.HasIndex(p => p.Slug).IsUnique();

            builder.Property(p => p.BasePrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.ViewCount)
                .HasDefaultValue(0);

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.ConfigureAudit();

            builder.HasMany(p => p.Variants)
                .WithOne()
                .HasForeignKey(v => v.ProductId);

            builder.HasMany(p => p.Images)
                .WithOne()
                .HasForeignKey(i => i.ProductId);
        }
    }
}