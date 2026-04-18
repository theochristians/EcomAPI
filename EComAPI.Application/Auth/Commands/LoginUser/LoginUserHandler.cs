using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Common.Security;
using EComAPI.Domain.Common.Exceptions;
using RefreshTokenEntity = EComAPI.Domain.Auth.Entities.RefreshToken;

namespace EComAPI.Application.Auth.Commands.LoginUser
{
    public class LoginUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ILoginHistoryRepository _loginHistoryRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public LoginUserHandler(
            IUserRepository userRepository,
            ILoginHistoryRepository loginHistoryRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _loginHistoryRepository = loginHistoryRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LoginUserDto>> Handle(
            LoginUserCommand loginUserCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(loginUserCommand.Email))
                    return Result<LoginUserDto>.Failure("Email is required");

                if (string.IsNullOrWhiteSpace(loginUserCommand.Password))
                    return Result<LoginUserDto>.Failure("Password is required");

                var normalizedEmail = loginUserCommand.Email.Trim().ToLowerInvariant();
                var userByEmail = await _userRepository.GetUserByEmailAsync(normalizedEmail, cancellationToken);

                if (userByEmail is null)
                    return Result<LoginUserDto>.Failure("Invalid credentials");

                if (!userByEmail.IsActive)
                    return Result<LoginUserDto>.Failure("Account is deactivated");

                if (!userByEmail.IsEmailVerified)
                    return Result<LoginUserDto>.Failure("Email is not verified. Please verify your email before login");

                if (!_passwordHasher.Verify(loginUserCommand.Password, userByEmail.Password.Value))
                    return Result<LoginUserDto>.Failure("Invalid credentials");

                var accessToken = await _jwtTokenGenerator.GenerateTokenAsync(userByEmail);
                var accessTokenExpiresAt = TokenHelper.ResolveAccessTokenExpiry(accessToken);
                var refreshTokenString = TokenHelper.GenerateRefreshToken();
                var refreshTokenHash = TokenHelper.HashToken(refreshTokenString);

                var refreshToken = new RefreshTokenEntity(
                    userByEmail.Id,
                    refreshTokenHash,
                    SecurityTime.UtcNow.AddDays(7),
                    userByEmail.Id,
                    loginUserCommand.IpAddress,
                    loginUserCommand.UserAgent,
                    loginUserCommand.DeviceName
                );

                var loginHistory = userByEmail.RecordLogin(
                    loginUserCommand.IpAddress,
                    loginUserCommand.UserAgent,
                    loginUserCommand.DeviceName);

                await _refreshTokenRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
                await _loginHistoryRepository.AddLoginHistoryAsync(loginHistory, cancellationToken);
                await _userRepository.UpdateUserAsync(userByEmail, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<LoginUserDto>.Success(new LoginUserDto(
                    accessToken,
                    refreshTokenString,
                    accessTokenExpiresAt,
                    refreshToken.ExpiresAt));
            }
            catch (DomainException domainException)
            {
                return Result<LoginUserDto>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<LoginUserDto>.Failure($"Failed to login: {exception.Message}");
            }
        }
    }
}

