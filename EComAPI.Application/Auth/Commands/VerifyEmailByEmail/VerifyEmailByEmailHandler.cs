using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.VerifyEmailByEmail
{
    public class VerifyEmailByEmailHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationRepository _emailVerificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyEmailByEmailHandler(
            IUserRepository userRepository,
            IEmailVerificationRepository emailVerificationRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailVerificationRepository = emailVerificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            VerifyEmailByEmailCommand verifyEmailByEmailCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var email = verifyEmailByEmailCommand.Email?.Trim().ToLowerInvariant() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(email))
                    return Result.Failure("Email is required");

                var code = verifyEmailByEmailCommand.Code?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(code))
                    return Result.Failure("Verification code is required");

                if (code.Length != 6)
                    return Result.Failure("Verification code must be 6 characters");

                var userByEmail = await _userRepository.GetUserByEmailAsync(email, cancellationToken);
                if (userByEmail is null)
                    return Result.Failure("Invalid email or verification code");

                if (userByEmail.IsEmailVerified)
                    return Result.Failure("Email is already verified");

                var latestPending = await _emailVerificationRepository.GetLatestPendingByUserIdAsync(
                    userByEmail.Id,
                    cancellationToken);

                if (latestPending is null)
                    return Result.Failure("Verification code not found. Please request a new code");

                if (latestPending.IsExpired)
                    return Result.Failure("Verification code has expired. Please request a new code");

                if (latestPending.IsMaxAttemptsReached)
                    return Result.Failure("Maximum verification attempts reached. Please request a new code");

                if (!string.Equals(latestPending.Code, code, StringComparison.Ordinal))
                {
                    latestPending.IncrementAttempt(SystemUsers.SystemUserId);
                    await _emailVerificationRepository.UpdateEmailVerificationAsync(latestPending, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return Result.Failure("Invalid verification code");
                }

                latestPending.MarkAsVerified(userByEmail.Id);
                userByEmail.VerifyEmail(userByEmail.Id);

                await _emailVerificationRepository.UpdateEmailVerificationAsync(latestPending, cancellationToken);
                await _userRepository.UpdateUserAsync(userByEmail, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (DomainException domainException)
            {
                return Result.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result.Failure($"Failed to verify email: {exception.Message}");
            }
        }
    }
}
