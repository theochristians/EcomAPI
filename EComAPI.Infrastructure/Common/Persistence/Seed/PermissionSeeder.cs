using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;
using EComAPI.Infrastructure.Common.Constants;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EComAPI.Infrastructure.Auth.Persistence.Seeders
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(
            AppDbContext appDbContext,
            IConfiguration configuration)
        {
            Console.WriteLine("Seeding Roles and Permissions...");

            await SeedSystemUserAsync(appDbContext);

            var adminRole = await GetOrCreateRoleAsync(appDbContext, "Admin");
            var customerRole = await GetOrCreateRoleAsync(appDbContext, "Customer");
            var permissions = new List<(string name, string category, string description)>
            {
                // Categories
                ("categories.read", "Category", "Read categories"),
                ("categories.create", "Category", "Create category"),
                ("categories.update", "Category", "Update category"),
                ("categories.delete", "Category", "Soft delete category"),
                ("categories.restore", "Category", "Restore category"),

                // Products
                ("products.read", "Product", "Read products"),
                ("products.create", "Product", "Create product"),
                ("products.update", "Product", "Update product"),
                ("products.delete", "Product", "Soft delete product"),
                ("products.restore", "Product", "Restore product"),

                // Users - Admin
                ("users.read.all", "User", "Admin: Read all users"),
                ("users.create", "User", "Admin: Create users"),
                ("users.update.any", "User", "Admin: Update any user"),
                ("users.delete.any", "User", "Admin: Soft delete any user"),
                ("users.restore.any", "User", "Admin: Restore any user"),
                ("users.harddelete.any", "User", "Admin: Permanently delete any user"),

                // Users - Customer
                ("users.read.own", "User", "Customer: Read own profile"),
                ("users.update.own", "User", "Customer: Update own profile"),
                ("users.delete.own", "User", "Customer: Permanently delete own account"),

                // Orders
                ("orders.read.all", "Order", "Admin: Read all orders"),
                ("orders.update.any", "Order", "Admin: Update any order"),
                ("orders.delete.any", "Order", "Admin: Delete any order"),
                ("orders.read.own", "Order", "Customer: Read own orders"),
                ("orders.create", "Order", "Customer: Create orders"),
                ("orders.cancel.own", "Order", "Customer: Cancel own orders"),
            };

            // ===== SEED PERMISSIONS =====
            var permissionEntities = new List<Permission>();
            foreach (var (name, category, description) in permissions)
            {
                var exists = await appDbContext.Permissions
                    .AnyAsync(permissions => permissions.Name == name);

                if (!exists)
                {
                    var permission = Permission.CreateForSeed(
                        Guid.NewGuid(),
                        name,
                        category,
                        SystemUsers.SystemUserId,
                        description
                    );

                    appDbContext.Permissions.Add(permission);
                    permissionEntities.Add(permission);
                    Console.WriteLine($"Added permission: {name}");
                }
                else
                {
                    var permission = await appDbContext.Permissions.FirstAsync(p => p.Name == name);
                    permissionEntities.Add(permission);
                }
            }

            await appDbContext.SaveChangesAsync();

            // ===== ASSIGN PERMISSIONS TO ADMIN =====
            Console.WriteLine("Assigning permissions to Admin role...");
            await AssignPermissionsToRoleAsync(appDbContext, adminRole, permissionEntities);

            // ===== ASSIGN PERMISSIONS TO CUSTOMER =====
            var customerPermissions = permissionEntities
                .Where(permissions =>
                    permissions.Name == "products.read" ||
                    permissions.Name == "categories.read" ||
                    permissions.Name.Contains("own") ||
                    permissions.Name == "orders.create"
                )
                .ToList();

            Console.WriteLine("Assigning permissions to Customer role...");
            await AssignPermissionsToRoleAsync(appDbContext, customerRole, customerPermissions);

            Console.WriteLine("Permission seeding completed!");
        }
        // ===== SEED SYSTEM USER =====
        private static async Task SeedSystemUserAsync(AppDbContext appDbContext)
        {
            var systemUserExists = await appDbContext.Users
                .IgnoreQueryFilters()
                .AnyAsync(user => user.Id == SystemUsers.SystemUserId);

            if (systemUserExists)
            {
                Console.WriteLine(" SYSTEM user already exists");
                return;
            }

            var adminRole = await appDbContext.Roles
                .FirstOrDefaultAsync(role => role.Name == "Admin");

            if (adminRole == null)
            {
                adminRole = Role.CreateForSeed(
                    Guid.NewGuid(),
                    "Admin",
                    SystemUsers.SystemUserId
                );
                appDbContext.Roles.Add(adminRole);
                await appDbContext.SaveChangesAsync();
            }

            var systemUser = User.CreateForSeed(
                SystemUsers.SystemUserId,
                SystemUsers.SystemUserName,
                EmailAddress.Create(SystemUsers.SystemUserEmail),
                PasswordHash.FromHash("SYSTEM_NOT_USABLE"),
                adminRole.Id,
                SystemUsers.SystemUserId
            );

            appDbContext.Users.Add(systemUser);
            await appDbContext.SaveChangesAsync();

            Console.WriteLine("Created SYSTEM user");
        }

        // ===== GET OR CREATE ROLE =====
        private static async Task<Role> GetOrCreateRoleAsync(
            AppDbContext appDbContext,
            string roleName)
        {
            var role = await appDbContext.Roles
                .FirstOrDefaultAsync(role => role.Name == roleName);

            if (role != null)
            {
                Console.WriteLine($"Role '{roleName}' already exists");
                return role;
            }

            role = Role.CreateForSeed(
                Guid.NewGuid(),
                roleName,
                SystemUsers.SystemUserId
            );

            appDbContext.Roles.Add(role);
            await appDbContext.SaveChangesAsync();

            Console.WriteLine($"Created role: {roleName}");
            return role;
        }

        // ===== ASSIGN PERMISSIONS TO ROLE =====
        private static async Task AssignPermissionsToRoleAsync(
            AppDbContext appDbContext,
            Role role,
            List<Permission> permissions)
        {
            int added = 0;
            int skipped = 0;

            foreach (var permission in permissions)
            {
                var exists = await appDbContext.RolePermissions.AnyAsync(rolePermission =>
                    rolePermission.RoleId == role.Id &&
                    rolePermission.PermissionId == permission.Id);

                if (!exists)
                {
                    appDbContext.RolePermissions.Add(
                        new RolePermission(role.Id, permission.Id, SystemUsers.SystemUserId)
                    );
                    added++;
                }
                else
                {
                    skipped++;
                }
            }

            await appDbContext.SaveChangesAsync();
            Console.WriteLine($"Assigned {added} permissions to '{role.Name}' (skipped {skipped} existing)");
        }

        private static bool? ParseNullableBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (bool.TryParse(value, out var parsedBool))
                return parsedBool;

            if (int.TryParse(value, out var parsedInt))
                return parsedInt != 0;

            return null;
        }

        private static string GetFirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }
    }
}
