using EComAPI.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Shopping.Persistence.Configurations
{
    public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
    {
        public void Configure(EntityTypeBuilder<Wishlist> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Wishlists");

            entityTypeBuilder.HasKey(wishlist => wishlist.Id);

            entityTypeBuilder.Property(wishlist => wishlist.UserId).IsRequired();

            entityTypeBuilder.Property(wishlist => wishlist.ProductId).IsRequired();

            entityTypeBuilder.HasIndex(wishlist => new { wishlist.UserId, wishlist.ProductId })
                .IsUnique()
                .HasFilter("[DeletedAt] IS NULL");

            entityTypeBuilder.Property(wishlist => wishlist.CreatedAt).IsRequired();
            entityTypeBuilder.Property(wishlist => wishlist.CreatedBy).IsRequired();
            entityTypeBuilder.Property(wishlist => wishlist.UpdatedAt).IsRequired(false);
            entityTypeBuilder.Property(wishlist => wishlist.UpdatedBy).IsRequired(false);
            entityTypeBuilder.Property(wishlist => wishlist.DeletedAt).IsRequired(false);
            entityTypeBuilder.Property(wishlist => wishlist.DeletedBy).IsRequired(false);
        }
    }
}
