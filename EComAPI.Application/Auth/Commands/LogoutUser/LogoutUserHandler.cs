using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Common.Security;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.LogoutUser
{
    public class LogoutUserHandler
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenBlacklistRepository _tokenBlacklistRepository;
        private readonly ITokenBlacklistLifetimeProvider _tokenBlacklistLifetimeProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public LogoutUserHandler(
            IRefreshTokenRepository refreshTokenRepository,
            ITokenBlacklistRepository tokenBlacklistRepository,
            ITokenBlacklistLifetimeProvider tokenBlacklistLifetimeProvider,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _tokenBlacklistRepository = tokenBlacklistRepository;
            _tokenBlacklistLifetimeProvider = tokenBlacklistLifetimeProvider;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            LogoutUserCommand logoutUserCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result.Failure("User not authenticated");

                if (!string.IsNullOrWhiteSpace(logoutUserCommand.RefreshToken))
                {
                    var refreshTokenHash = TokenHelper.HashToken(logoutUserCommand.RefreshToken);
                    var refreshToken = await _refreshTokenRepository.GetRefreshTokenAsync(refreshTokenHash, cancellationToken);

                    if (refreshToken != null)
                    {
                        if (refreshToken.UserId != _currentUser.UserId)
                            return Result.Failure("Refresh token does not belong to current user");

                        if (refreshToken.IsActive)
                        {
                            refreshToken.Revoke("User logged out", _currentUser.UserId);
                            await _refreshTokenRepository.UpdateRefreshTokenAsync(refreshToken, cancellationToken);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(logoutUserCommand.AccessToken))
                {
                    var accessTokenHash = TokenHelper.HashToken(logoutUserCommand.AccessToken);

                    var isTokenBlacklisted = await _tokenBlacklistRepository.IsBlacklistedAsync(
                        accessTokenHash,
                        cancellationToken);

                    if (!isTokenBlacklisted)
                    {
                        var expiresAt = TokenHelper.ResolveAccessTokenExpiry(
                            logoutUserCommand.AccessToken,
                            _tokenBlacklistLifetimeProvider.FallbackLifetime,
                            _tokenBlacklistLifetimeProvider.MaxLifetime);

                        var tokenBlacklist = new TokenBlacklist(
                            _currentUser.UserId,
                            accessTokenHash,
                            "User logged out",
                            expiresAt,
                            _currentUser.UserId
                        );

                        await _tokenBlacklistRepository.AddTokenBlacklistAsync(tokenBlacklist, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DomainException domainException)
            {
                return Result.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result.Failure($"Logout failed: {exception.Message}");
            }
        }
    }
}
