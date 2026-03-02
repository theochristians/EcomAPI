using EComAPI.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
    {
        public void Configure(EntityTypeBuilder<EmailVerification> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("EmailVerification");

            entityTypeBuilder.HasKey(emailVerification => emailVerification.Id);

            entityTypeBuilder.Property(emailVerification => emailVerification.UserId)
                .IsRequired();

            entityTypeBuilder.Property(emailVerification => emailVerification.Code)
                .IsRequired()
                .HasMaxLength(6);

            entityTypeBuilder.Property(emailVerification => emailVerification.ExpiresAt)
                .IsRequired();

            entityTypeBuilder.Property(emailVerification => emailVerification.VerifiedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(emailVerification => emailVerification.Attempts)
                .HasDefaultValue(0)
                .IsRequired();

            entityTypeBuilder.HasOne(emailVerification => emailVerification.User)
                .WithMany(user => user.EmailVerifications)
                .HasForeignKey(emailVerification => emailVerification.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(emailVerification => emailVerification.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(emailVerification => emailVerification.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(emailVerification => emailVerification.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(emailVerification => emailVerification.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(emailVerification => emailVerification.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(emailVerification => emailVerification.DeletedBy)
                .IsRequired(false);
        }
    }
}
