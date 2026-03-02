using EComAPI.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
    {
        public void Configure(EntityTypeBuilder<LoginHistory> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("LoginHistories");

            entityTypeBuilder.HasKey(loginHistory => loginHistory.Id);

            entityTypeBuilder.Property(loginHistory => loginHistory.UserId)
                .IsRequired();

            entityTypeBuilder.Property(loginHistory => loginHistory.Status)
                .IsRequired()
                .HasMaxLength(20);

            entityTypeBuilder.Property(loginHistory => loginHistory.FailureReason)
                .IsRequired(false)
                .HasMaxLength(500);

            entityTypeBuilder.Property(loginHistory => loginHistory.IpAddress)
                .IsRequired(false)
                .HasMaxLength(64);

            entityTypeBuilder.Property(loginHistory => loginHistory.UserAgent)
                .IsRequired(false)
                .HasMaxLength(500);

            entityTypeBuilder.Property(loginHistory => loginHistory.DeviceName)
                .IsRequired(false)
                .HasMaxLength(200);

            entityTypeBuilder.HasOne(loginHistory => loginHistory.User)
                .WithMany(user => user.LoginHistories)
                .HasForeignKey(loginHistory => loginHistory.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(loginHistory => loginHistory.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(loginHistory => loginHistory.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(loginHistory => loginHistory.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(loginHistory => loginHistory.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(loginHistory => loginHistory.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(loginHistory => loginHistory.DeletedBy)
                .IsRequired(false);
        }
    }
}
