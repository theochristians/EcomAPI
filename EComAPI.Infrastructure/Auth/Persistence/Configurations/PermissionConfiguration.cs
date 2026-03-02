using EComAPI.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Permissions");

            entityTypeBuilder.HasKey(perrmission => perrmission.Id);

            entityTypeBuilder.Property(perrmission => perrmission.Name)
                .IsRequired()
                .HasMaxLength(150);

            entityTypeBuilder.HasIndex(perrmission => perrmission.Name)
                .IsUnique();

            entityTypeBuilder.Property(perrmission => perrmission.Category)
                .IsRequired()
                .HasMaxLength(100);

            entityTypeBuilder.Property(perrmission => perrmission.Description)
                .IsRequired(false);

            entityTypeBuilder.Property(perrmission => perrmission.IsActive)
                .IsRequired();

            entityTypeBuilder.HasMany(perrmission => perrmission.RolePermissions)
                .WithOne(rolePermission => rolePermission.Permission)
                .HasForeignKey(rolePermission => rolePermission.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder.Property(perrmission => perrmission.CreatedAt)
                .IsRequired();

            entityTypeBuilder.Property(perrmission => perrmission.CreatedBy)
                .IsRequired();

            entityTypeBuilder.Property(perrmission => perrmission.UpdatedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(perrmission => perrmission.UpdatedBy)
                .IsRequired(false);

            entityTypeBuilder.Property(perrmission => perrmission.DeletedAt)
                .IsRequired(false);

            entityTypeBuilder.Property(perrmission => perrmission.DeletedBy)
                .IsRequired(false);
        }
    }
}