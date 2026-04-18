using EComAPI.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace EComAPI.Infrastructure.Auth.Security
{
    public sealed class TokenBlacklistLifetimeProvider : ITokenBlacklistLifetimeProvider
    {
        private const int DefaultFallbackMinutes = 15;
        private const int DefaultMaxMinutes = 24 * 60;

        public TimeSpan FallbackLifetime { get; }
        public TimeSpan MaxLifetime { get; }

        public TokenBlacklistLifetimeProvider(IConfiguration configuration)
        {
            var fallbackMinutes = configuration.GetValue<int?>("TokenBlacklist:FallbackMinutes")
                ?? DefaultFallbackMinutes;

            if (fallbackMinutes < 1)
                fallbackMinutes = DefaultFallbackMinutes;

            var maxMinutes = configuration.GetValue<int?>("TokenBlacklist:MaxMinutes")
                ?? DefaultMaxMinutes;

            if (maxMinutes < 1)
                maxMinutes = DefaultMaxMinutes;

            if (fallbackMinutes > maxMinutes)
                fallbackMinutes = maxMinutes;

            FallbackLifetime = TimeSpan.FromMinutes(fallbackMinutes);
            MaxLifetime = TimeSpan.FromMinutes(maxMinutes);
        }
    }
}
