using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Infrastructure.Common.Identity;
using EComAPI.Infrastructure.Common.Persistence;
using EComAPI.Infrastructure.Common.Persistence.Context;
using EComAPI.Infrastructure.Auth.Persistence.Repositories;
using EComAPI.Infrastructure.Auth.Security;
using EComAPI.Infrastructure.Categories.Persistence.Repositories;
using EComAPI.Infrastructure.Products.Persistence.Repositories;

namespace EComAPI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            serviceCollection.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("SQLServerECom")));
            serviceCollection.AddHttpContextAccessor();
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
            serviceCollection.AddScoped<ICurrentUser, CurrentUser>();
            serviceCollection.Configure<JwtSettings>(
                configuration.GetSection(JwtSettings.SectionName));

            // ===== AUTH REPOSITORIES =====
            serviceCollection.AddScoped<IUserRepository, UserRepository>();
            serviceCollection.AddScoped<IRoleRepository, RoleRepository>();
            serviceCollection.AddScoped<IPermissionRepository, PermissionRepository>();
            serviceCollection.AddScoped<IAddressRepository, AddressRepository>();
            serviceCollection.AddScoped<ITokenBlacklistRepository, TokenBlacklistRepository>();
            serviceCollection.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            serviceCollection.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();

            // ===== PRODUCT REPOSITORIES =====
            serviceCollection.AddScoped<IProductRepository, ProductRepository>();

            // ===== CATEGORY REPOSITORIES =====
            serviceCollection.AddScoped<ICategoryRepository, CategoryRepository>();

            // ===== SECURITY SERVICES =====
            serviceCollection.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            serviceCollection.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            serviceCollection.AddScoped<IEmailSender, ConsoleEmailSender>();

            return serviceCollection;
        }
    }
}
