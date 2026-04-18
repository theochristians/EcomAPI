using EComAPI.Application.Auth.Interfaces;
using Microsoft.Extensions.Logging;

namespace EComAPI.Infrastructure.Auth.Security
{
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _logger;

        public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailVerificationCodeAsync(
            string toEmail,
            string fullName,
            string code,
            DateTime expiresAt,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Email verification code for {Email} ({FullName}): {Code}. Expires at {ExpiresAtUtc:u}",
                toEmail,
                fullName,
                code,
                expiresAt);

            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(
            string toEmail,
            string fullName,
            string code,
            DateTime expiresAt,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Password reset code for {Email} ({FullName}): {Code}. Expires at {ExpiresAtUtc:u}",
                toEmail,
                fullName,
                code,
                expiresAt);

            return Task.CompletedTask;
        }
    }
}
