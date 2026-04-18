using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class ReviewImageConfiguration : IEntityTypeConfiguration<ReviewImage>
    {
        public void Configure(EntityTypeBuilder<ReviewImage> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("ReviewImages");

            entityTypeBuilder.HasKey(reviewImage => reviewImage.Id);

            entityTypeBuilder.Property(reviewImage => reviewImage.ReviewId).IsRequired();

            entityTypeBuilder.Property(reviewImage => reviewImage.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            entityTypeBuilder.Property(reviewImage => reviewImage.DisplayOrder).IsRequired();

            entityTypeBuilder.Property(reviewImage => reviewImage.CreatedAt).IsRequired();

            entityTypeBuilder.HasIndex(reviewImage => reviewImage.ReviewId);
        }
    }
}
