using EComAPI.Infrastructure.Auth.Persistence.Seeders;
using EComAPI.Infrastructure.Common.Persistence.Context;

namespace EComAPI.API.Startup;

public static class DatabaseSeedingExtensions
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<AppDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();

        try
        {
            await PermissionSeeder.SeedAsync(dbContext, configuration);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"? Seeding error: {exception.Message}");
            throw;
        }
    }
}
