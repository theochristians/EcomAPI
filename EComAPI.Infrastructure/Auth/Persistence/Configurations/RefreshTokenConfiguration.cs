using EComAPI.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> entityTypeBuilder) 
        {
            entityTypeBuilder.ToTable("RefreshTokens");

            entityTypeBuilder.HasKey(refreshToken => refreshToken.Id); 

            entityTypeBuilder.Property(refreshToken => refreshToken.UserId)
                .IsRequired();

            entityTypeBuilder.Property(refreshToken => refreshToken.TokenHash)
                .IsRequired()
                .HasMaxLength(500);

            entityTypeBuilder.HasIndex(refreshToken => refreshToken.TokenHash)
                .IsUnique();

            entityTypeBuilder.Property(refreshToken => refreshToken.ExpiresAt)
                .IsRequired();

            entityTypeBuilder.Property(refreshToken => refreshToken.RevokedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(refreshToken => refreshToken.RevokeReason)
                .HasMaxLength(500);

            entityTypeBuilder.Property(refreshToken => refreshToken.IpAddress)
                .HasMaxLength(64);

            entityTypeBuilder.Property(refreshToken => refreshToken.UserAgent)
                .HasMaxLength(500);

            entityTypeBuilder.Property(refreshToken => refreshToken.DeviceName)
                .HasMaxLength(200);

            entityTypeBuilder.HasOne(refreshToken => refreshToken.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(refreshToken => refreshToken.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(refreshToken => refreshToken.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(refreshToken => refreshToken.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(refreshToken => refreshToken.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(refreshToken => refreshToken.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(refreshToken => refreshToken.DeletedBy)
                .IsRequired(false);
        }
    }
}