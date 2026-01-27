using Microsoft.Extensions.DependencyInjection;

namespace PatternNET.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Otomatis daftarkan semua Command/Query Handler
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}