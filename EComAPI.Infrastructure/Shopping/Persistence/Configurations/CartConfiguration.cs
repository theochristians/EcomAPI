using EComAPI.Domain.Shopping.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Shopping.Persistence.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Carts");

            entityTypeBuilder.HasKey(cart => cart.Id);

            entityTypeBuilder.Property(cart => cart.UserId)
                .IsRequired();

            entityTypeBuilder.HasIndex(cart => cart.UserId)
                .IsUnique();

            entityTypeBuilder.HasMany(cart => cart.Items)
                .WithOne(cartItem => cartItem.Cart)
                .HasForeignKey(cartItem => cartItem.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(cart => cart.CreatedAt).IsRequired();
            entityTypeBuilder.Property(cart => cart.CreatedBy).IsRequired();
            entityTypeBuilder.Property(cart => cart.UpdatedAt).IsRequired(false);
            entityTypeBuilder.Property(cart => cart.UpdatedBy).IsRequired(false);
            entityTypeBuilder.Property(cart => cart.DeletedAt).IsRequired(false);
            entityTypeBuilder.Property(cart => cart.DeletedBy).IsRequired(false);
        }
    }
}
