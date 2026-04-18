using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using EComAPI.Infrastructure.Common.Persistence.Context;

namespace EComAPI.Infrastructure.Common.Persistence
{
    public class AppDbContextFactory
        : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var basePath = Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "EComAPI.API"
            );

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true);

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                configurationBuilder.AddJsonFile(
                    $"appsettings.{environmentName}.json",
                    optional: true);
            }

            var configuration = configurationBuilder
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("SQLServerECom")
                ?? configuration.GetConnectionString("SQLServerThrifted")
                ?? throw new InvalidOperationException(
                    "Connection string is missing. Configure ConnectionStrings:SQLServerECom or ConnectionStrings:SQLServerThrifted.");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
