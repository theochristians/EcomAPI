using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Categories.Entities;
using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Product.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EComAPI.Infrastructure.Common.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Auth
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        // Products
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();

        // Categories
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // ✅ GLOBAL QUERY FILTER - Auto filter soft deleted entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Check if entity inherits from SoftDeletableEntity
                if (typeof(SoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(SoftDeletableEntity.DeletedAt));
                    var condition = Expression.Equal(property, Expression.Constant(null, typeof(DateTime?)));
                    var lambda = Expression.Lambda(condition, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // ✅ AUTO-SET AUDIT FIELDS
            var entries = ChangeTracker.Entries<AuditableEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    // Set CreatedAt if not already set
                    if (entry.Entity.CreatedAt == default)
                    {
                        entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = DateTime.UtcNow;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Always update UpdatedAt on modification
                    entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            // ✅ AUTO-SET AUDIT FIELDS (sync version)
            var entries = ChangeTracker.Entries<AuditableEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity.CreatedAt == default)
                    {
                        entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = DateTime.UtcNow;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
                }
            }

            return base.SaveChanges();
        }
    }
}