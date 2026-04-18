using EComAPI.Infrastructure.Auth.Persistence.Seeders;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.Infrastructure.Common.Persistence.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Console.WriteLine("Starting database seeding...");

            await PermissionSeeder.SeedAsync(
                appDbContext,
                scope.ServiceProvider.GetRequiredService<IConfiguration>());

            Console.WriteLine("Database seeding completed!");
        }
    }
}
