using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class TokenBlacklistConfiguration : IEntityTypeConfiguration<TokenBlacklist>
    {
        public void Configure(EntityTypeBuilder<TokenBlacklist> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("TokenBlacklists");

            entityTypeBuilder.HasKey(tokenBlacklist => tokenBlacklist.Id);

            entityTypeBuilder.Property(tokenBlacklist => tokenBlacklist.UserId)
                .IsRequired();

            entityTypeBuilder.Property(tokenBlacklist => tokenBlacklist.TokenHash)
                .IsRequired()
                .HasMaxLength(500);

            entityTypeBuilder.HasIndex(tokenBlacklist => tokenBlacklist.TokenHash);

            entityTypeBuilder.Property(tokenBlacklist => tokenBlacklist.Reason)
                .IsRequired()
                .HasMaxLength(500);

            entityTypeBuilder.Property(tokenBlacklist => tokenBlacklist.ExpiresAt)
                .IsRequired();

            entityTypeBuilder.HasOne(tokenBlacklist => tokenBlacklist.User)
                .WithMany(user => user.TokenBlacklists)
                .HasForeignKey(tokenBlacklist => tokenBlacklist.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(tokenBlacklist => tokenBlacklist.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(tokenBlacklist => tokenBlacklist.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(tokenBlacklist => tokenBlacklist.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(tokenBlacklist => tokenBlacklist.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(tokenBlacklist => tokenBlacklist.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(tokenBlacklist => tokenBlacklist.DeletedBy)
                .IsRequired(false);
        }
    }
}