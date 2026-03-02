using EComAPI.Domain.Categories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Categories.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Categories");

            entityTypeBuilder.HasKey(category => category.Id);

            entityTypeBuilder.Property(category => category.Name)
                .IsRequired()
                .HasMaxLength(150);

            entityTypeBuilder.Property(category => category.Slug)
                .IsRequired()
                .HasMaxLength(150);

            entityTypeBuilder.HasIndex(category => category.Slug)
                .IsUnique();

            entityTypeBuilder.Property(category => category.ParentId)
                .IsRequired(false);

            entityTypeBuilder.Property(category => category.ImageUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            entityTypeBuilder.Property(category => category.Description)
                .IsRequired(false)
                .HasMaxLength(1000);

            entityTypeBuilder.Property(category => category.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(category => category.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(category => category.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(category => category.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(category => category.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(category => category.DeletedBy)
                .IsRequired(false);

            entityTypeBuilder.HasOne(category => category.Parent)
                .WithMany(category => category.Children)
                .HasForeignKey(category => category.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}