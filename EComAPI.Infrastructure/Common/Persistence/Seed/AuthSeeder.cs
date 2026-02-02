using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Common.Persistence.Seed
{
    public static class AuthSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // ===== ROLE =====
            if (!await context.Roles.AnyAsync())
            {
                var adminRole = new Role("Admin");
                var customerRole = new Role("Customer");

                context.Roles.AddRange(adminRole, customerRole);
                await context.SaveChangesAsync();
            }

            // ===== PERMISSION =====
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new[]
                {
                    new Permission("auth.login", "auth"),
                    new Permission("auth.register", "auth"),

                    new Permission("product.read", "product"),
                    new Permission("product.create", "product"),
                    new Permission("product.update", "product"),
                    new Permission("product.delete", "product"),

                    new Permission("order.read", "order"),
                    new Permission("order.create", "order"),
                    new Permission("order.update", "order"),

                    new Permission("user.read", "user"),
                    new Permission("user.manage", "user")
                };

                context.Permissions.AddRange(permissions);
                await context.SaveChangesAsync();
            }

            // ===== ROLE_PERMISSION =====
            if (!await context.RolePermissions.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
                var customerRole = await context.Roles.FirstAsync(r => r.Name == "Customer");

                var permissions = await context.Permissions.ToListAsync();

                var rolePermissions = new List<RolePermission>();

                foreach (var permission in permissions)
                {
                    // Admin gets all
                    rolePermissions.Add(new RolePermission(adminRole.Id, permission.Id));

                    // Customer limited
                    if (permission.Name is "auth.login" or "auth.register" or "product.read" or "order.create" or "order.read")
                    {
                        rolePermissions.Add(new RolePermission(customerRole.Id, permission.Id));
                    }
                }

                context.RolePermissions.AddRange(rolePermissions);
                await context.SaveChangesAsync();
            }
        }
    }
}
