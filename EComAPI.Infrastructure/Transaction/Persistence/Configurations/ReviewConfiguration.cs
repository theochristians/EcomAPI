using EComAPI.Domain.Transaction.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Transaction.Persistence.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Reviews");

            entityTypeBuilder.HasKey(review => review.Id);

            entityTypeBuilder.Property(review => review.UserId).IsRequired();
            entityTypeBuilder.Property(review => review.OrderId).IsRequired();
            entityTypeBuilder.Property(review => review.ProductId).IsRequired();

            entityTypeBuilder.HasOne<Order>()
                .WithMany()
                .HasForeignKey(review => review.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entityTypeBuilder.Property(review => review.Rating).IsRequired();

            entityTypeBuilder.Property(review => review.Comment)
                .HasMaxLength(2000)
                .IsRequired(false);

            entityTypeBuilder.HasMany(review => review.Images)
                .WithOne()
                .HasForeignKey(reviewImage => reviewImage.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.HasIndex(review => new { review.UserId, review.OrderId, review.ProductId }).IsUnique();
            entityTypeBuilder.HasIndex(review => review.OrderId);
            entityTypeBuilder.HasIndex(review => review.ProductId);

            entityTypeBuilder.Property(review => review.CreatedAt).IsRequired();
            entityTypeBuilder.Property(review => review.CreatedBy).IsRequired();
            entityTypeBuilder.Property(review => review.UpdatedAt).IsRequired(false);
            entityTypeBuilder.Property(review => review.UpdatedBy).IsRequired(false);
            entityTypeBuilder.Property(review => review.DeletedAt).IsRequired(false);
            entityTypeBuilder.Property(review => review.DeletedBy).IsRequired(false);
        }
    }
}
