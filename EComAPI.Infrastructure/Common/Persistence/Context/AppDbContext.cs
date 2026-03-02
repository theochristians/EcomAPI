using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Categories.Entities; // ✅ ADD THIS LINE!
using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Products.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace EComAPI.Infrastructure.Common.Persistence.Context
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<TokenBlacklist> TokenBlacklists => Set<TokenBlacklist>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
        public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();
        public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        private static LambdaExpression BuildSoftDeleteFilter(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
            var condition = Expression.Equal(property, Expression.Constant(null, typeof(DateTime?)));

            return Expression.Lambda(condition, parameter);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            SetAuditFields();
            return base.SaveChanges();
        }

        private void SetAuditFields()
        {
            var currentUserId = GetCurrentUserId();
            var now = DateTime.UtcNow;

            var auditableEntries = ChangeTracker.Entries<AuditableEntity>();

            foreach (var entry in auditableEntries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = now;

                    if (currentUserId.HasValue)
                    {
                        entry.Property(nameof(AuditableEntity.CreatedBy)).CurrentValue = currentUserId.Value;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = now;

                    if (currentUserId.HasValue)
                    {
                        entry.Property(nameof(AuditableEntity.UpdatedBy)).CurrentValue = currentUserId.Value;
                    }

                    entry.Property(nameof(AuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;
                }
            }

            var softDeletableEntries = ChangeTracker.Entries<SoftDeletableEntity>();

            foreach (var entry in softDeletableEntries)
            {
                if (entry.State == EntityState.Modified)
                {
                    var deletedAtProperty = entry.Property(nameof(SoftDeletableEntity.DeletedAt));

                    if (deletedAtProperty.IsModified &&
                        deletedAtProperty.CurrentValue != null)
                    {
                        var deletedByProperty = entry.Property(nameof(SoftDeletableEntity.DeletedBy));

                        if (deletedByProperty.CurrentValue == null && currentUserId.HasValue)
                        {
                            deletedByProperty.CurrentValue = currentUserId.Value;
                        }
                    }
                }
            }
        }

        private Guid? GetCurrentUserId()
        {
            if (_httpContextAccessor?.HttpContext == null)
                return null;

            var user = _httpContextAccessor.HttpContext.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
                return null;

            var subClaim = user.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(subClaim) && Guid.TryParse(subClaim, out var subGuid))
            {
                return subGuid;
            }

            var nameIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(nameIdClaim) && Guid.TryParse(nameIdClaim, out var nameGuid))
            {
                return nameGuid;
            }

            return null;
        }
    }
}