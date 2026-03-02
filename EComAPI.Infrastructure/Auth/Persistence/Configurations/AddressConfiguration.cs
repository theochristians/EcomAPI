using EComAPI.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Addresses");

            entityTypeBuilder.HasKey(address => address.Id); 

            entityTypeBuilder.Property(address => address.UserId) 
                .IsRequired();

            entityTypeBuilder.Property(address => address.Label)
                .IsRequired()
                .HasMaxLength(50);

            entityTypeBuilder.Property(address => address.RecipientName)
                .IsRequired()
                .HasMaxLength(100);

            entityTypeBuilder.Property(address => address.RecipientPhone)
                .IsRequired()
                .HasMaxLength(20);

            entityTypeBuilder.Property(address => address.FullAddress)
                .IsRequired()
                .HasMaxLength(500);

            entityTypeBuilder.Property(address => address.City)
                .IsRequired()
                .HasMaxLength(100);

            entityTypeBuilder.Property(address => address.Province)
                .IsRequired()
                .HasMaxLength(100);

            entityTypeBuilder.Property(address => address.PostalCode)
                .IsRequired()
                .HasMaxLength(10);

            entityTypeBuilder.Property(address => address.IsDefault)
                .HasDefaultValue(false)
                .IsRequired();

            entityTypeBuilder.HasOne(address => address.User)
                .WithMany(user => user.Addresses)
                .HasForeignKey(address => address.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(address => address.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(address => address.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(address => address.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(address => address.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(address => address.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(address => address.DeletedBy)
                .IsRequired(false);
        }
    }
}