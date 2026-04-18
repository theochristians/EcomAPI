using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Common.Security;
using EComAPI.Domain.Auth.ValueObjects;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.ResetPassword
{
    public class ResetPasswordHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ResetPasswordHandler(
            IUserRepository userRepository,
            IPasswordResetRepository passwordResetRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordResetRepository = passwordResetRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            ResetPasswordCommand resetPasswordCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var normalizedEmail = resetPasswordCommand.Email?.Trim().ToLowerInvariant() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(normalizedEmail))
                    return Result.Failure("Email is required");

                var code = resetPasswordCommand.Code?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(code))
                    return Result.Failure("Reset code is required");

                if (code.Length != 6)
                    return Result.Failure("Reset code must be 6 characters");

                if (string.IsNullOrWhiteSpace(resetPasswordCommand.NewPassword))
                    return Result.Failure("New password is required");

                if (resetPasswordCommand.NewPassword.Length < 8)
                    return Result.Failure("New password must be at least 8 characters");

                var userByEmail = await _userRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);
                if (userByEmail is null || !userByEmail.IsActive)
                    return Result.Failure("Invalid reset code or email");

                var resetCodeHash = TokenHelper.HashToken($"{userByEmail.Id}:{code}");
                var passwordReset = await _passwordResetRepository.GetByUserIdAndTokenHashAsync(
                    userByEmail.Id,
                    resetCodeHash,
                    cancellationToken);

                if (passwordReset is null)
                    return Result.Failure("Invalid reset code or email");

                if (passwordReset.IsExpired)
                    return Result.Failure("Reset code has expired");

                if (passwordReset.IsUsed)
                    return Result.Failure("Reset code has already been used");

                var hashedPassword = _passwordHasher.Hash(resetPasswordCommand.NewPassword);
                var newPassword = PasswordHash.FromHash(hashedPassword);

                userByEmail.UpdatePassword(newPassword, userByEmail.Id);
                passwordReset.MarkAsUsed(userByEmail.Id);

                var activeRefreshTokens = await _refreshTokenRepository.GetActiveByUserIdAsync(
                    userByEmail.Id,
                    cancellationToken);

                foreach (var activeRefreshToken in activeRefreshTokens)
                {
                    activeRefreshToken.Revoke("Revoked after password reset", userByEmail.Id);
                    await _refreshTokenRepository.UpdateRefreshTokenAsync(activeRefreshToken, cancellationToken);
                }

                await _userRepository.UpdateUserAsync(userByEmail, cancellationToken);
                await _passwordResetRepository.UpdatePasswordResetAsync(passwordReset, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (DomainException domainException)
            {
                return Result.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result.Failure($"Failed to reset password: {exception.Message}");
            }
        }
    }
}
