using System.Security.Cryptography;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.SendEmailVerification
{
    public class SendEmailVerificationHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationRepository _emailVerificationRepository;
        private readonly IEmailSender _emailSender;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public SendEmailVerificationHandler(
            IUserRepository userRepository,
            IEmailVerificationRepository emailVerificationRepository,
            IEmailSender emailSender,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _emailVerificationRepository = emailVerificationRepository;
            _emailSender = emailSender;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DateTime>> Handle(
            SendEmailVerificationCommand sendEmailVerificationCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<DateTime>.Failure("User not authenticated");

                var userById = await _userRepository.GetUserByIdAsync(_currentUser.UserId, cancellationToken);
                if (userById is null)
                    return Result<DateTime>.Failure("User not found");

                if (userById.IsEmailVerified)
                    return Result<DateTime>.Failure("Email is already verified");

                var latestPending = await _emailVerificationRepository.GetLatestPendingByUserIdAsync(
                    userById.Id,
                    cancellationToken);

                if (latestPending is not null)
                {
                    latestPending.Delete(_currentUser.UserId);
                    await _emailVerificationRepository.UpdateEmailVerificationAsync(latestPending, cancellationToken);
                }

                var verificationCode = GenerateVerificationCode();
                var expiresAt = DateTime.UtcNow.AddMinutes(10);

                var emailVerification = new EmailVerification(
                    userById.Id,
                    verificationCode,
                    expiresAt,
                    _currentUser.UserId);

                await _emailVerificationRepository.AddEmailVerificationAsync(emailVerification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _emailSender.SendEmailVerificationCodeAsync(
                    userById.Email.Value,
                    userById.FullName,
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

        private static string GenerateVerificationCode()
        {
            Span<byte> randomBytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(randomBytes);

            var randomValue = BitConverter.ToUInt32(randomBytes) % 1_000_000;
            return randomValue.ToString("D6");
        }
    }
}
