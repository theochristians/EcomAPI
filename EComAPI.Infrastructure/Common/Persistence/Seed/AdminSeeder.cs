using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;
using EComAPI.Infrastructure.Common.Constants;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Common.Persistence.Seed
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
            AppDbContext appDbContext,
            IPasswordHasher passwordHasher)
        {
            var adminEmail = "theochristian.sir@ecomapi.com";

            var adminExists = await appDbContext.Users
                .AnyAsync(user => user.Email.Value == adminEmail);

            if (adminExists)
            {
                Console.WriteLine("✅ Admin user already exists");
                return;
            }

            Console.WriteLine("🔧 Creating default admin user...");

            var adminRole = await appDbContext.Roles
                .FirstOrDefaultAsync(role => role.Name == "Admin");

            if (adminRole == null)
            {
                Console.WriteLine("❌ Admin role not found! Run PermissionSeeder first.");
                return;
            }

            var email = EmailAddress.Create(adminEmail);
            var hashedPassword = passwordHasher.Hash("Admin123!");
            var password = PasswordHash.FromHash(hashedPassword);

            var adminUser = new User(
                fullName: "System Administrator",
                emailAddress: email,
                password: password,
                roleId: adminRole.Id,
                createdBy: SystemUsers.SystemUserId, 
                phone: null
            );

            adminUser.VerifyEmail(SystemUsers.SystemUserId);

            appDbContext.Users.Add(adminUser);
            await appDbContext.SaveChangesAsync();

            Console.WriteLine("✅ Admin user created successfully!");
            Console.WriteLine($"   Email: {adminEmail}");
            Console.WriteLine($"   Password: Admin123!");
            Console.WriteLine("   ⚠️  PLEASE CHANGE PASSWORD AFTER FIRST LOGIN!");
        }
    }
}