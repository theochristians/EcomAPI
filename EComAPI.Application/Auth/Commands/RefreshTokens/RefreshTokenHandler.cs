using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Common.Security;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.RefreshTokens
{
    public class RefreshTokenHandler
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenBlacklistRepository _tokenBlacklistRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenHandler(
            IRefreshTokenRepository refreshTokenRepository,
            ITokenBlacklistRepository tokenBlacklistRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _tokenBlacklistRepository = tokenBlacklistRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<LoginUserDto>> Handle(
            RefreshTokenCommand refreshTokenCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshTokenCommand.RefreshToken))
                    return Result<LoginUserDto>.Failure("Refresh token is required");

                var tokenHash = TokenHelper.HashToken(refreshTokenCommand.RefreshToken);
                var refreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(tokenHash, cancellationToken);

                if (refreshToken == null)
                    return Result<LoginUserDto>.Failure("Invalid refresh token");

                if (!refreshToken.IsActive)
                {
                    if (refreshToken.IsRevoked)
                        return Result<LoginUserDto>.Failure("Refresh token has been revoked");

                    if (refreshToken.IsExpired)
                        return Result<LoginUserDto>.Failure("Refresh token has expired");
                }

                // If bearer token exists, enforce same user with refresh token
                if (_currentUser.IsAuthenticated && refreshToken.UserId != _currentUser.UserId)
                    return Result<LoginUserDto>.Failure("Refresh token does not belong to current user");

                var user = refreshToken.User;
                if (user == null)
                    return Result<LoginUserDto>.Failure("User not found");

                if (!user.IsActive)
                    return Result<LoginUserDto>.Failure("Account is deactivated");

                // Revoke old refresh token (rotation)
                refreshToken.Revoke("Token rotated", user.Id);
                await _refreshTokenRepository.UpdateRefreshTokenAsync(refreshToken, cancellationToken);

                // Optional: blacklist current access token so old access token stops immediately
                if (!string.IsNullOrWhiteSpace(refreshTokenCommand.CurrentAccessToken))
                {
                    var oldAccessTokenHash = TokenHelper.HashToken(refreshTokenCommand.CurrentAccessToken);
                    var isOldAccessTokenBlacklisted = await _tokenBlacklistRepository.IsBlacklistedAsync(
                        oldAccessTokenHash,
                        cancellationToken);

                    if (!isOldAccessTokenBlacklisted)
                    {
                        var tokenBlacklist = new TokenBlacklist(
                            user.Id,
                            oldAccessTokenHash,
                            "Access token rotated",
                            DateTime.UtcNow.AddMinutes(15),
                            user.Id
                        );

                        await _tokenBlacklistRepository.AddTokenBlacklistAsync(tokenBlacklist, cancellationToken);
                    }
                }

                var newAccessToken = await _jwtTokenGenerator.GenerateTokenAsync(user);
                var newRefreshTokenString = TokenHelper.GenerateRefreshToken();
                var newRefreshTokenHash = TokenHelper.HashToken(newRefreshTokenString);

                // Gunakan metadata dari request saat ini,
                // fallback ke metadata token lama jika tidak tersedia.
                var newRefreshToken = new RefreshToken(
                    user.Id,
                    newRefreshTokenHash,
                    DateTime.UtcNow.AddDays(7),
                    user.Id,
                    refreshTokenCommand.IpAddress ?? refreshToken.IpAddress,
                    refreshTokenCommand.UserAgent ?? refreshToken.UserAgent,
                    refreshTokenCommand.DeviceName ?? refreshToken.DeviceName
                );

                await _refreshTokenRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<LoginUserDto>.Success(new LoginUserDto(
                    newAccessToken,
                    newRefreshTokenString,
                    DateTime.UtcNow.AddMinutes(15),
                    newRefreshToken.ExpiresAt
                ));
            }
            catch (DomainException domainException)
            {
                return Result<LoginUserDto>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<LoginUserDto>.Failure($"Failed to refresh token: {exception.Message}");
            }
        }
    }
}
