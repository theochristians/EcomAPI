using EComAPI.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Shopping.Persistence.Configurations
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("CartItems");

            entityTypeBuilder.HasKey(cartItem => cartItem.Id);

            entityTypeBuilder.Property(cartItem => cartItem.CartId).IsRequired();

            entityTypeBuilder.Property(cartItem => cartItem.ProductVariantId).IsRequired();

            entityTypeBuilder.Property(cartItem => cartItem.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            entityTypeBuilder.HasOne(cartItem => cartItem.Cart)
                .WithMany(cart => cart.Items)
                .HasForeignKey(cartItem => cartItem.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(cartItem => cartItem.CreatedAt).IsRequired();
            entityTypeBuilder.Property(cartItem => cartItem.CreatedBy).IsRequired();
            entityTypeBuilder.Property(cartItem => cartItem.UpdatedAt).IsRequired(false);
            entityTypeBuilder.Property(cartItem => cartItem.UpdatedBy).IsRequired(false);
            entityTypeBuilder.Property(cartItem => cartItem.DeletedAt).IsRequired(false);
            entityTypeBuilder.Property(cartItem => cartItem.DeletedBy).IsRequired(false);
        }
    }
}
