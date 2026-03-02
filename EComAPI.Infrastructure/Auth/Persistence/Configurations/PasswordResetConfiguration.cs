using EComAPI.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class PasswordResetConfiguration : IEntityTypeConfiguration<PasswordReset>
    {
        public void Configure(EntityTypeBuilder<PasswordReset> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("PasswordResets");

            entityTypeBuilder.HasKey(passwordReset => passwordReset.Id);

            entityTypeBuilder.Property(passwordReset => passwordReset.UserId)
                .IsRequired();

            entityTypeBuilder.Property(passwordReset => passwordReset.TokenHash)
                .IsRequired()
                .HasMaxLength(500);

            entityTypeBuilder.HasIndex(passwordReset => passwordReset.TokenHash)
                .IsUnique();

            entityTypeBuilder.Property(passwordReset => passwordReset.ExpiresAt)
                .IsRequired();

            entityTypeBuilder.Property(passwordReset => passwordReset.UsedAt)
                .IsRequired(false);

            entityTypeBuilder.HasOne(passwordReset => passwordReset.User)
                .WithMany(user => user.PasswordResets)
                .HasForeignKey(passwordReset => passwordReset.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(passwordReset => passwordReset.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(passwordReset => passwordReset.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(passwordReset => passwordReset.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(passwordReset => passwordReset.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(passwordReset => passwordReset.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(passwordReset => passwordReset.DeletedBy)
                .IsRequired(false);
        }
    }
}
