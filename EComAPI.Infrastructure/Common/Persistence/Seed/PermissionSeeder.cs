using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Auth.Persistence.Seeders
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // =========================
            // PERMISSION DEFINITIONS
            // =========================
            var permissions = new List<Permission>
            {
                // ===== CATEGORY =====
                new("categories.read", "Category", "Read categories"),
                new("categories.create", "Category", "Create category"),
                new("categories.update", "Category", "Update category"),
                new("categories.delete", "Category", "Delete category"),
                new("categories.restore", "Category", "Restore category"),

                // ===== PRODUCT =====
                new("products.read", "Product", "Read products"),
                new("products.create", "Product", "Create product"),
                new("products.update", "Product", "Update product"),
                new("products.delete", "Product", "Delete product"),
                new("products.restore", "Product", "Restore product")
            };

            foreach (var permission in permissions)
            {
                var exists = await context.Permissions
                    .AnyAsync(p => p.Name == permission.Name);

                if (!exists)
                {
                    context.Permissions.Add(permission);
                }
            }

            await context.SaveChangesAsync();

            // =========================
            // ROLE SEEDING
            // =========================
            var adminRole = await GetOrCreateRole(context, "Admin");
            var customerRole = await GetOrCreateRole(context, "Customer");

            // =========================
            // ASSIGN PERMISSIONS TO ADMIN
            // =========================
            await AssignPermissionsToRole(context, adminRole, permissions);
        }

        // =========================
        // HELPERS
        // =========================
        private static async Task<Role> GetOrCreateRole(
            AppDbContext context,
            string roleName)
        {
            var role = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName);

            if (role != null)
                return role;

            role = new Role(roleName);
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            return role;
        }

        private static async Task AssignPermissionsToRole(
            AppDbContext context,
            Role role,
            List<Permission> permissions)
        {
            foreach (var permission in permissions)
            {
                var permissionEntity = await context.Permissions
                    .FirstAsync(p => p.Name == permission.Name);

                var exists = await context.RolePermissions.AnyAsync(rp =>
                    rp.RoleId == role.Id &&
                    rp.PermissionId == permissionEntity.Id);

                if (!exists)
                {
                    context.RolePermissions.Add(
                        new RolePermission(role.Id, permissionEntity.Id)
                    );
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
