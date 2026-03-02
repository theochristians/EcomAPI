using EComAPI.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("RolePermissions");

            entityTypeBuilder.HasKey(rolePermission => rolePermission.Id);

            entityTypeBuilder.Property(rolePermission => rolePermission.RoleId)
                .IsRequired();

            entityTypeBuilder.Property(rolePermission => rolePermission.PermissionId)
                .IsRequired();

            entityTypeBuilder.HasIndex(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId })
                .IsUnique();

            entityTypeBuilder.HasOne(rolePermission => rolePermission.Role)
                .WithMany(role => role.RolePermissions)
                .HasForeignKey(rolePermission => rolePermission.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.HasOne(rolePermission => rolePermission.Permission)
                .WithMany(permission => permission.RolePermissions)
                .HasForeignKey(rolePermission => rolePermission.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(rolePermission => rolePermission.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(rolePermission => rolePermission.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(rolePermission => rolePermission.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(rolePermission => rolePermission.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(rolePermission => rolePermission.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(rolePermission => rolePermission.DeletedBy)
                .IsRequired(false);
        }
    }
}