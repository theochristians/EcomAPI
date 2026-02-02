using EComAPI.Domain.Common.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EComAPI.Infrastructure.Common.Persistence.Configurations
{
    public static class AuditableConfiguration
    {
        public static void ConfigureAudit<TEntity>(
            this EntityTypeBuilder<TEntity> builder)
            where TEntity : AuditableEntity
        {
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.CreatedBy).IsRequired(false);
            builder.Property(x => x.UpdatedAt).IsRequired(false);
            builder.Property(x => x.UpdatedBy).IsRequired(false);
        }

        /// <summary>
        /// Configure soft delete property for entities that inherit from SoftDeletableEntity
        /// </summary>
        public static void ConfigureSoftDelete<TEntity>(
            this EntityTypeBuilder<TEntity> builder)
            where TEntity : SoftDeletableEntity
        {
            // Configure audit fields first
            builder.ConfigureAudit();

            // Configure soft delete
            builder.Property(x => x.DeletedAt).IsRequired(false);

            // Create index on DeletedAt for better query performance
            builder.HasIndex(x => x.DeletedAt);
        }
    }
}