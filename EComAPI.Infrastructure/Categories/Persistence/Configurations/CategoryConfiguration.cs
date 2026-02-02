using EComAPI.Domain.Categories.Entities;
using EComAPI.Infrastructure.Common.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Categories.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(x => x.Slug)
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");

            builder.Property(x => x.ParentId)
                .IsRequired(false);

            builder.ConfigureSoftDelete();
        }
    }
}