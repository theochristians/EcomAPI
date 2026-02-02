using EComAPI.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Auth.Persistence.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RoleId).IsRequired();
            builder.Property(x => x.PermissionId).IsRequired();

            builder.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
        }
    }
}