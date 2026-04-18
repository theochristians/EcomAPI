using EComAPI.Application.Auth.Common;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.SendEmailVerificationByEmail
{
    public class SendEmailVerificationByEmailHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationRepository _emailVerificationRepository;
        private readonly IEmailSender _emailSender;
        private readonly IUnitOfWork _unitOfWork;

        public SendEmailVerificationByEmailHandler(
            IUserRepository userRepository,
            IEmailVerificationRepository emailVerificationRepository,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailVerificationRepository = emailVerificationRepository;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DateTime>> Handle(
            SendEmailVerificationByEmailCommand sendEmailVerificationByEmailCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sendEmailVerificationByEmailCommand.Email))
                    return Result<DateTime>.Failure("Email is required");

                var normalizedEmail = sendEmailVerificationByEmailCommand.Email.Trim().ToLowerInvariant();
                var userByEmail = await _userRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);

                var fallbackExpiresAt = SecurityTime.UtcNow.AddMinutes(10);
                if (userByEmail is null || !userByEmail.IsActive)
                    return Result<DateTime>.Success(fallbackExpiresAt);

                if (userByEmail.IsEmailVerified)
                    return Result<DateTime>.Failure("Email is already verified");

                var latestPending = await _emailVerificationRepository.GetLatestPendingByUserIdAsync(
                    userByEmail.Id,
                    cancellationToken);

                if (latestPending is not null)
                {
                    latestPending.Delete(SystemUsers.SystemUserId);
                    await _emailVerificationRepository.UpdateEmailVerificationAsync(latestPending, cancellationToken);
                }

                var verificationCode = OtpCodeGenerator.GenerateSixDigits();
                var expiresAt = SecurityTime.UtcNow.AddMinutes(10);

                var emailVerification = new EmailVerification(
                    userByEmail.Id,
                    verificationCode,
                    expiresAt,
                    SystemUsers.SystemUserId);

                await _emailVerificationRepository.AddEmailVerificationAsync(emailVerification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _emailSender.SendEmailVerificationCodeAsync(
                    userByEmail.Email.Value,
                    userByEmail.FullName,
                    verificationCode,
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
                return Result<DateTime>.Failure($"Failed to send verification email: {exception.Message}");
            }
        }
    }
}
