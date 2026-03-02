using EComAPI.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Users");

            entityTypeBuilder.HasKey(user => user.Id);

            entityTypeBuilder.Property(user => user.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entityTypeBuilder.OwnsOne(user => user.Email, email =>
            {
                email.Property(emailAddress => emailAddress.Value)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);

                email.HasIndex(emailAddress => emailAddress.Value)
                    .IsUnique();
            });

            entityTypeBuilder.OwnsOne(user => user.Password, password =>
            {
                password.Property(passwordHash => passwordHash.Value)
                    .HasColumnName("PasswordHash")
                    .IsRequired();
            });

            entityTypeBuilder.Property(user => user.RoleId)
                .IsRequired();

            entityTypeBuilder.Property(user => user.IsActive)
                .IsRequired();

            entityTypeBuilder.Property(user => user.IsEmailVerified)
                .IsRequired();

            entityTypeBuilder.HasOne(user => user.Role)
                .WithMany(role => role.Users)
                .HasForeignKey(user => user.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entityTypeBuilder.Property(user => user.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(user => user.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(user => user.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(user => user.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(user => user.DeletedAt)
                .IsRequired(false);
        }
    }
}