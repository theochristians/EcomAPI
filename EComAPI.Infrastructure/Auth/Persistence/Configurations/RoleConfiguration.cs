using EComAPI.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Roles");

            entityTypeBuilder.HasKey(role => role.Id);

            entityTypeBuilder.Property(role => role.Name)
                .IsRequired()
                .HasMaxLength(100);

            entityTypeBuilder.HasIndex(role => role.Name)
                .IsUnique();

            entityTypeBuilder.HasMany(role => role.Users)
                .WithOne(user => user.Role)
                .HasForeignKey(user => user.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entityTypeBuilder.HasMany(role => role.RolePermissions)
                .WithOne(rolePermission => rolePermission.Role)
                .HasForeignKey(rolePermission => rolePermission.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(role => role.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(role => role.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(role => role.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(role => role.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(role => role.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(role => role.DeletedBy)
                .IsRequired(false);
        }
    }
}