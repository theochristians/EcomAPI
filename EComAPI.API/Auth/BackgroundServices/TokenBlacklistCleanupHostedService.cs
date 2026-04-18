using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;

namespace EComAPI.API.Auth.BackgroundServices
{
    public sealed class TokenBlacklistCleanupHostedService : BackgroundService
    {
        private const int DefaultIntervalMinutes = 60;
        private const int MinIntervalMinutes = 5;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<TokenBlacklistCleanupHostedService> _logger;
        private readonly TimeSpan _interval;

        public TokenBlacklistCleanupHostedService(
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            ILogger<TokenBlacklistCleanupHostedService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;

            var configuredInterval = configuration.GetValue<int?>("TokenBlacklist:CleanupIntervalMinutes")
                ?? DefaultIntervalMinutes;

            if (configuredInterval < MinIntervalMinutes)
                configuredInterval = MinIntervalMinutes;

            _interval = TimeSpan.FromMinutes(configuredInterval);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Token blacklist cleanup worker started with interval {CleanupIntervalMinutes} minutes.",
                _interval.TotalMinutes);

            await RunCleanupAsync(stoppingToken);

            using var timer = new PeriodicTimer(_interval);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var hasNextTick = await timer.WaitForNextTickAsync(stoppingToken);
                    if (!hasNextTick)
                        break;

                    await RunCleanupAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task RunCleanupAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var blacklistRepository = scope.ServiceProvider.GetRequiredService<ITokenBlacklistRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await blacklistRepository.CleanupExpiredTokensAsync(cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to cleanup expired token blacklist entries.");
            }
        }
    }
}
