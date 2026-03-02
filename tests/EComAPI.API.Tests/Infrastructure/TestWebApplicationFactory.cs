using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EComAPI.Infrastructure.Common.Persistence.Context;

namespace EComAPI.API.Tests.Infrastructure
{
    /// <summary>
    /// Custom WebApplicationFactory untuk integration tests.
    /// Mengganti SQL Server dengan InMemory DB agar test bisa berjalan tanpa koneksi database.
    /// </summary>
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = $"EComTest_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                // ── Hapus registrasi AppDbContext yang pakai SQL Server ──
                services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
                services.RemoveAll(typeof(AppDbContext));

                // ── Tambah AppDbContext dengan InMemory DB ──
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                    options.EnableSensitiveDataLogging();
                });
            });
        }

        /// <summary>
        /// Buat scope baru dan kembalikan AppDbContext-nya untuk setup/verify test data.
        /// </summary>
        public AppDbContext CreateDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }
    }
}
