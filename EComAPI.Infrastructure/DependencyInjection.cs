using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Infrastructure.Auth.Persistence.Repositories;
using EComAPI.Infrastructure.Auth.Security;
using EComAPI.Infrastructure.Categories.Persistence.Repositories;
using EComAPI.Infrastructure.Common.Caching;
using EComAPI.Infrastructure.Common.Identity;
using EComAPI.Infrastructure.Common.Persistence;
using EComAPI.Infrastructure.Common.Persistence.Context;
using EComAPI.Infrastructure.Common.Storage;
using EComAPI.Infrastructure.Products.Persistence.Repositories;
using EComAPI.Infrastructure.Shopping.Persistence.Repositories;
using EComAPI.Infrastructure.Transaction.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                    configuration.GetConnectionString("SQLServerThrifted")));

            serviceCollection.AddHttpContextAccessor();
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
            serviceCollection.AddScoped<ICurrentUser, CurrentUser>();
            serviceCollection.AddSingleton<ITokenBlacklistLifetimeProvider, TokenBlacklistLifetimeProvider>();
            serviceCollection.Configure<JwtSettings>(
                configuration.GetSection(JwtSettings.SectionName));
            var emailOptions = BuildEmailOptions(configuration);
            serviceCollection.AddSingleton(emailOptions);
            var azureBlobStorageOptions = BuildAzureBlobStorageOptions(configuration);
            serviceCollection.AddSingleton(azureBlobStorageOptions);

            // Cache (Upstash Redis REST dengan fallback memory)
            serviceCollection.AddMemoryCache();
            var redisOptions = BuildRedisOptions(configuration);
            serviceCollection.AddSingleton(redisOptions);

            if (redisOptions.Enabled)
            {
                serviceCollection.AddHttpClient<ICacheService, UpstashRedisCacheService>(client =>
                {
                    client.BaseAddress = new Uri($"{redisOptions.Url.TrimEnd('/')}/");
                    client.Timeout = TimeSpan.FromSeconds(10);
                });
            }
            else
            {
                serviceCollection.AddSingleton<ICacheService, InMemoryCacheService>();
            }

            serviceCollection.AddScoped<ICacheInvalidationBuffer, CacheInvalidationBuffer>();
            serviceCollection.AddScoped<ICacheWriteBuffer, CacheWriteBuffer>();

            // Auth Repositories
            serviceCollection.AddScoped<IUserRepository, UserRepository>();
            serviceCollection.AddScoped<IRoleRepository, RoleRepository>();
            serviceCollection.AddScoped<IPermissionRepository, PermissionRepository>();
            serviceCollection.AddScoped<IAddressRepository, AddressRepository>();
            serviceCollection.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();
            serviceCollection.AddScoped<ITokenBlacklistRepository, TokenBlacklistRepository>();
            serviceCollection.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            serviceCollection.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
            serviceCollection.AddScoped<IPasswordResetRepository, PasswordResetRepository>();

            // Product Repositories
            serviceCollection.AddScoped<IProductRepository, ProductRepository>();

            // Category Repositories
            serviceCollection.AddScoped<ICategoryRepository, CategoryRepository>();

            // Shopping Repositories
            serviceCollection.AddScoped<ICartRepository, CartRepository>();
            serviceCollection.AddScoped<IWishlistRepository, WishlistRepository>();

            // Transaction Repositories
            serviceCollection.AddScoped<IOrderRepository, OrderRepository>();
            serviceCollection.AddScoped<ICouponRepository, CouponRepository>();
            serviceCollection.AddScoped<IReviewRepository, ReviewRepository>();
            serviceCollection.AddScoped<IReturnRepository, ReturnRepository>();
            serviceCollection.AddScoped<IOrderStatusLogRepository, OrderStatusLogRepository>();
            serviceCollection.AddScoped<IStockLogRepository, StockLogRepository>();

            // Other Services
            serviceCollection.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            serviceCollection.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            if (azureBlobStorageOptions.Enabled)
            {
                serviceCollection.AddSingleton<IFileStorageService, AzureBlobFileStorageService>();
            }
            else
            {
                serviceCollection.AddSingleton<IFileStorageService, DisabledFileStorageService>();
            }
            if (emailOptions.Enabled)
            {
                serviceCollection.AddScoped<IEmailSender, BrevoSmtpEmailSender>();
            }
            else
            {
                serviceCollection.AddScoped<IEmailSender, ConsoleEmailSender>();
            }

            return serviceCollection;
        }

        private static AzureBlobStorageOptions BuildAzureBlobStorageOptions(IConfiguration configuration)
        {
            var accountName = GetFirstNonEmpty(
                configuration["AzureBlobStorage:AccountName"],
                configuration["AZURE_STORAGE_ACCOUNT_NAME"]);

            var containerName = GetFirstNonEmpty(
                configuration["AzureBlobStorage:ContainerName"],
                configuration["AZURE_STORAGE_CONTAINER"]);

            var tenantId = GetFirstNonEmpty(
                configuration["AzureBlobStorage:TenantId"],
                configuration["AZURE_TENANT_ID"]);

            var clientId = GetFirstNonEmpty(
                configuration["AzureBlobStorage:ClientId"],
                configuration["AZURE_CLIENT_ID"]);

            var clientSecret = GetFirstNonEmpty(
                configuration["AzureBlobStorage:ClientSecret"],
                configuration["AZURE_CLIENT_SECRET"]);

            var configuredEnabled = configuration.GetValue<bool?>("AzureBlobStorage:Enabled");
            var enabledByCredentials =
                !string.IsNullOrWhiteSpace(accountName) &&
                !string.IsNullOrWhiteSpace(containerName);

            var enabled = configuredEnabled ?? enabledByCredentials;
            if (!enabledByCredentials)
                enabled = false;

            return new AzureBlobStorageOptions
            {
                Enabled = enabled,
                AccountName = accountName,
                ContainerName = containerName,
                TenantId = tenantId,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
        }

        private static EmailOptions BuildEmailOptions(IConfiguration configuration)
        {
            var smtpHost = GetFirstNonEmpty(
                configuration["Email:SmtpHost"],
                configuration["EMAIL_SMTP_HOST"],
                "smtp-relay.brevo.com");

            var smtpPort = GetFirstInt(
                configuration["Email:SmtpPort"],
                configuration["EMAIL_SMTP_PORT"],
                587);

            var smtpUsername = GetFirstNonEmpty(
                configuration["Email:SmtpUsername"],
                configuration["EMAIL_SMTP_USERNAME"]);

            var smtpPassword = GetFirstNonEmpty(
                configuration["Email:SmtpPassword"],
                configuration["EMAIL_SMTP_PASSWORD"]);

            var fromEmail = GetFirstNonEmpty(
                configuration["Email:FromEmail"],
                configuration["EMAIL_FROM_EMAIL"]);

            var fromName = GetFirstNonEmpty(
                configuration["Email:FromName"],
                configuration["EMAIL_FROM_NAME"],
                "EComAPI");

            var brandName = GetFirstNonEmpty(
                configuration["Email:BrandName"],
                configuration["EMAIL_BRAND_NAME"],
                "Thrifties");

            var brandDomain = GetFirstNonEmpty(
                configuration["Email:BrandDomain"],
                configuration["EMAIL_BRAND_DOMAIN"],
                "thrifties.app");

            var logoUrl = GetFirstNonEmpty(
                configuration["Email:LogoUrl"],
                configuration["EMAIL_LOGO_URL"]);

            var configuredEnableSsl = configuration.GetValue<bool?>("Email:EnableSsl");
            var envEnableSsl = ParseNullableBool(configuration["EMAIL_ENABLE_SSL"]);
            var enableSsl = configuredEnableSsl ?? envEnableSsl ?? true;

            var configuredEnabled = configuration.GetValue<bool?>("Email:Enabled");
            var envEnabled = ParseNullableBool(configuration["EMAIL_ENABLED"]);

            var hasRequiredCredentials =
                !string.IsNullOrWhiteSpace(smtpHost) &&
                smtpPort > 0 &&
                !string.IsNullOrWhiteSpace(smtpUsername) &&
                !string.IsNullOrWhiteSpace(smtpPassword) &&
                !string.IsNullOrWhiteSpace(fromEmail);

            var enabled = configuredEnabled ?? envEnabled ?? hasRequiredCredentials;

            if (!hasRequiredCredentials)
                enabled = false;

            return new EmailOptions
            {
                Enabled = enabled,
                SmtpHost = smtpHost,
                SmtpPort = smtpPort,
                SmtpUsername = smtpUsername,
                SmtpPassword = smtpPassword,
                FromEmail = fromEmail,
                FromName = fromName,
                EnableSsl = enableSsl,
                BrandName = brandName,
                BrandDomain = brandDomain,
                LogoUrl = logoUrl
            };
        }

        private static UpstashRedisOptions BuildRedisOptions(IConfiguration configuration)
        {
            var redisUrl = configuration["Redis:Url"];
            var envRedisUrl = configuration["REDIS_URL"];
            var url = !string.IsNullOrWhiteSpace(redisUrl) ? redisUrl : (envRedisUrl ?? string.Empty);
            var redisToken = configuration["Redis:Token"];
            var envRedisToken = configuration["REDIS_TOKEN"];
            var token = !string.IsNullOrWhiteSpace(redisToken) ? redisToken : (envRedisToken ?? string.Empty);
            var configuredEnabled = configuration.GetValue<bool?>("Redis:Enabled");
            var enabledByCredentials = !string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(token);
            var enabled = configuredEnabled ?? enabledByCredentials;

            if (!enabledByCredentials)
                enabled = false;

            var instanceName = configuration["Redis:InstanceName"] ?? "ecomapi:";

            if (!instanceName.EndsWith(':'))
                instanceName += ":";

            return new UpstashRedisOptions
            {
                Enabled = enabled,
                Url = url,
                Token = token,
                InstanceName = instanceName
            };
        }

        private static string GetFirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }

        private static int GetFirstInt(string? primary, string? secondary, int fallback)
        {
            if (int.TryParse(primary, out var primaryValue) && primaryValue > 0)
                return primaryValue;

            if (int.TryParse(secondary, out var secondaryValue) && secondaryValue > 0)
                return secondaryValue;

            return fallback;
        }

        private static bool? ParseNullableBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (bool.TryParse(value, out var parsed))
                return parsed;

            return null;
        }
    }
}

