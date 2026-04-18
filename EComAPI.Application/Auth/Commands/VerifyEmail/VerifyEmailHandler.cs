using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;
using System.Text;

namespace EComAPI.Application.Auth.Commands.VerifyEmail
{
    public class VerifyEmailHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationRepository _emailVerificationRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyEmailHandler(
            IUserRepository userRepository,
            IEmailVerificationRepository emailVerificationRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailVerificationRepository = emailVerificationRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            VerifyEmailCommand verifyEmailCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result.Failure("User not authenticated");

                var code = NormalizeVerificationCode(verifyEmailCommand.Code);
                if (string.IsNullOrWhiteSpace(code))
                    return Result.Failure("Verification code is required");

                if (code.Length != 6)
                    return Result.Failure("Verification code must be 6 characters");

                var userById = await _userRepository.GetUserByIdAsync(_currentUser.UserId, cancellationToken);
                if (userById is null)
                    return Result.Failure("User not found");

                if (userById.IsEmailVerified)
                    return Result.Failure("Email is already verified");

                var latestPending = await _emailVerificationRepository.GetLatestPendingByUserIdAsync(
                    userById.Id,
                    cancellationToken);

                if (latestPending is null)
                    return Result.Failure("Verification code not found. Please request a new code");

                if (latestPending.IsExpired)
                    return Result.Failure("Verification code has expired. Please request a new code");

                if (latestPending.IsMaxAttemptsReached)
                    return Result.Failure("Maximum verification attempts reached. Please request a new code");

                if (!string.Equals(NormalizeVerificationCode(latestPending.Code), code, StringComparison.OrdinalIgnoreCase))
                {
                    latestPending.IncrementAttempt(_currentUser.UserId);
                    await _emailVerificationRepository.UpdateEmailVerificationAsync(latestPending, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return Result.Failure("Invalid verification code");
                }

                latestPending.MarkAsVerified(_currentUser.UserId);
                userById.VerifyEmail(_currentUser.UserId);

                await _emailVerificationRepository.UpdateEmailVerificationAsync(latestPending, cancellationToken);
                await _userRepository.UpdateUserAsync(userById, cancellationToken);
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

        private static string NormalizeVerificationCode(string? rawCode)
        {
            if (string.IsNullOrWhiteSpace(rawCode))
                return string.Empty;

            var builder = new StringBuilder(rawCode.Length);
            foreach (var character in rawCode)
            {
                if (char.IsLetterOrDigit(character))
                    builder.Append(character);
            }

            return builder.ToString();
        }
    }
}
