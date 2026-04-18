using EComAPI.Application.Auth.Common;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Common.Security;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;

        public ForgotPasswordHandler(
            IUserRepository userRepository,
            IPasswordResetRepository passwordResetRepository,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordResetRepository = passwordResetRepository;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DateTime>> Handle(
            ForgotPasswordCommand forgotPasswordCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(forgotPasswordCommand.Email))
                    return Result<DateTime>.Failure("Email is required");

                var normalizedEmail = forgotPasswordCommand.Email.Trim().ToLowerInvariant();
                var userByEmail = await _userRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);

                var fallbackExpiresAt = SecurityTime.UtcNow.AddMinutes(10);
                if (userByEmail is null || !userByEmail.IsActive)
                    return Result<DateTime>.Success(fallbackExpiresAt);

                var latestPending = await _passwordResetRepository.GetLatestPendingByUserIdAsync(
                    userByEmail.Id,
                    cancellationToken);

                if (latestPending is not null)
                {
                    latestPending.Delete(SystemUsers.SystemUserId);
                    await _passwordResetRepository.UpdatePasswordResetAsync(latestPending, cancellationToken);
                }

                var resetCode = OtpCodeGenerator.GenerateSixDigits();
                var resetCodeHash = TokenHelper.HashToken($"{userByEmail.Id}:{resetCode}");
                var expiresAt = SecurityTime.UtcNow.AddMinutes(10);

                var passwordReset = new PasswordReset(
                    userByEmail.Id,
                    resetCodeHash,
                    expiresAt,
                    SystemUsers.SystemUserId);

                await _passwordResetRepository.AddPasswordResetAsync(passwordReset, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _emailSender.SendPasswordResetCodeAsync(
                    userByEmail.Email.Value,
                    userByEmail.FullName,
                    resetCode,
                    expiresAt,
                    cancellationToken);

                return Result<DateTime>.Success(expiresAt);
            }
            catch (DomainException domainException)
            {
                return Result<DateTime>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<DateTime>.Failure($"Failed to send password reset code: {exception.Message}");
            }
        }
    }
}
