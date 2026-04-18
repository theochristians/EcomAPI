using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Infrastructure.Auth.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using EComAPI.Infrastructure.Common.Persistence.Context;

namespace EComAPI.API.Tests.Infrastructure
{
    /// <summary>
    /// Custom WebApplicationFactory for integration tests.
    /// Replaces SQL Server with InMemory DB and disables external side effects.
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"EComTest_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            });

            builder.ConfigureServices(services =>
            {
                // Force email sender to non-network implementation for deterministic tests.
                services.RemoveAll(typeof(IEmailSender));
                services.AddScoped<IEmailSender, ConsoleEmailSender>();

                // Replace SQL Server AppDbContext with in-memory AppDbContext.
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(AppDbContext));

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                    options.EnableSensitiveDataLogging();
                });
            });
        }

        /// <summary>
        /// Creates a new scope and returns AppDbContext for test setup/verification.
        /// </summary>
        public AppDbContext CreateDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }
    }
}
