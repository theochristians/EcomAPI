using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class ReturnImageConfiguration : IEntityTypeConfiguration<ReturnImage>
    {
        public void Configure(EntityTypeBuilder<ReturnImage> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("ReturnImages");

            entityTypeBuilder.HasKey(returnImage => returnImage.Id);

            entityTypeBuilder.Property(returnImage => returnImage.ReturnId).IsRequired();

            entityTypeBuilder.Property(returnImage => returnImage.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            entityTypeBuilder.Property(returnImage => returnImage.Description)
                .HasMaxLength(255)
                .IsRequired(false);

            entityTypeBuilder.Property(returnImage => returnImage.CreatedAt).IsRequired();

            entityTypeBuilder.HasIndex(returnImage => returnImage.ReturnId);
        }
    }
}
