using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.Infrastructure.Common.Persistence.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await AuthSeeder.SeedAsync(context);
            await ProductSeeder.SeedAsync(context);
        }
    }
}