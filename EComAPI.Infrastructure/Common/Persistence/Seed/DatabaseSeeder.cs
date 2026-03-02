using EComAPI.Application.Auth.Interfaces;
using EComAPI.Infrastructure.Auth.Persistence.Seeders;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.Infrastructure.Common.Persistence.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            Console.WriteLine("🌱 Starting database seeding...");

            await PermissionSeeder.SeedAsync(appDbContext);
            await AdminSeeder.SeedAsync(appDbContext, passwordHasher);

            Console.WriteLine("✅ Database seeding completed!");
        }
    }
}