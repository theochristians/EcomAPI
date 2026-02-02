
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Infrastructure.Auth.Persistence.Repositories;
using EComAPI.Infrastructure.Auth.Security;
using EComAPI.Infrastructure.Categories.Persistence.Repositories;
using EComAPI.Infrastructure.Common.Persistence;
using EComAPI.Infrastructure.Products.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("SQLServerECom")));

            services.Configure<JwtSettings>(
                configuration.GetSection(JwtSettings.SectionName));

            // Auth
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();

            // Product
            services.AddScoped<IProductRepository, ProductRepository>();

            // Category
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            // Security
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}
