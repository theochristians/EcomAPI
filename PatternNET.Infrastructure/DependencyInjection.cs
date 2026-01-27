using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatternNET.Application.Interfaces;
using PatternNET.Infrastructure.Persistence;
using PatternNET.Infrastructure.Persistence.Repositories;

namespace PatternNET.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Sambungkan Interface Repository ke Implementation aslinya
        services.AddScoped<IContactMessageRepository, ContactMessageRepository>();

        return services;
    }
}