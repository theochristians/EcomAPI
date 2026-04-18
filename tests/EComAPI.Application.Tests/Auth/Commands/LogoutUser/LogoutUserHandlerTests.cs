using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Auth.Commands.LogoutUser;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Tests.Auth.Commands.LogoutUser
{
    public class LogoutUserHandlerTests
    {
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private readonly Mock<ITokenBlacklistRepository> _mockTokenBlacklistRepository;
        private readonly Mock<ITokenBlacklistLifetimeProvider> _mockTokenBlacklistLifetimeProvider;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly LogoutUserHandler _logoutUserHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public LogoutUserHandlerTests()
        {
            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _mockTokenBlacklistRepository = new Mock<ITokenBlacklistRepository>();
            _mockTokenBlacklistLifetimeProvider = new Mock<ITokenBlacklistLifetimeProvider>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _mockTokenBlacklistLifetimeProvider.SetupGet(x => x.FallbackLifetime).Returns(TimeSpan.FromMinutes(15));
            _mockTokenBlacklistLifetimeProvider.SetupGet(x => x.MaxLifetime).Returns(TimeSpan.FromHours(24));

            _logoutUserHandler = new LogoutUserHandler(
                _mockRefreshTokenRepository.Object,
                _mockTokenBlacklistRepository.Object,
                _mockTokenBlacklistLifetimeProvider.Object,
                _mockCurrentUser.Object,
                _mockUnitOfWork.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private RefreshToken BuildActiveToken(Guid? userId = null) => new RefreshToken(
            userId ?? _userId,
            "hashed_token_value",
            DateTime.UtcNow.AddDays(7),
            userId ?? _userId
        );

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var logoutUserCommand = new LogoutUserCommand(RefreshToken: null, AccessToken: null);
            var logoutUserResult = await _logoutUserHandler.Handle(logoutUserCommand);

            logoutUserResult.IsSuccess.Should().BeFalse();
            logoutUserResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_TanpaToken_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();

            var logoutUserCommand = new LogoutUserCommand(RefreshToken: null, AccessToken: null);
            var logoutUserResult = await _logoutUserHandler.Handle(logoutUserCommand);

            logoutUserResult.IsSuccess.Should().BeTrue();
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RefreshTokenTidakDitemukan_ShouldStillSucceed()
        {
            SetupAuthenticatedUser();

            _mockRefreshTokenRepository
                .Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

            var logoutUserCommand = new LogoutUserCommand(RefreshToken: "some_refresh_token", AccessToken: null);
            var logoutUserResult = await _logoutUserHandler.Handle(logoutUserCommand);

            logoutUserResult.IsSuccess.Should().BeTrue();
            _mockRefreshTokenRepository.Verify(
                x => x.UpdateRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_RefreshTokenMilikUserLain_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var tokenMilikUserLain = BuildActiveToken(userId: Guid.NewGuid());

            _mockRefreshTokenRepository
                .Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenMilikUserLain);

            var logoutUserCommand = new LogoutUserCommand(RefreshToken: "someone_elses_token", AccessToken: null);
            var logoutUserResult = await _logoutUserHandler.Handle(logoutUserCommand);

            logoutUserResult.IsSuccess.Should().BeFalse();
            logoutUserResult.Error.Should().Be("Refresh token does not belong to current user");
        }

        [Fact]
        public async Task Handle_RefreshTokenValid_ShouldRevokeToken()
        {
            SetupAuthenticatedUser();

            var activeToken = BuildActiveToken();

            _mockRefreshTokenRepository
                .Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeToken);

            var logoutUserCommand = new LogoutUserCommand(RefreshToken: "valid_refresh_token", AccessToken: null);
            var logoutUserResult = await _logoutUserHandler.Handle(logoutUserCommand);

            logoutUserResult.IsSuccess.Should().BeTrue();
            _mockRefreshTokenRepository.Verify(
                x => x.UpdateRefreshTokenAsync(activeToken, It.IsAny<CancellationToken>()),
                Times.Once);
            activeToken.IsRevoked.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_AccessTokenBelumDiBlacklist_ShouldBlacklistToken()
        {
            SetupAuthenticatedUser();

            _mockTokenBlacklistRepository
                .Setup(x => x.IsBlacklistedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var logoutUserCommand = new LogoutUserCommand(RefreshToken: null, AccessToken: "valid_access_token");
            var logoutUserResult = await _logoutUserHandler.Handle(logoutUserCommand);

            logoutUserResult.IsSuccess.Should().BeTrue();
            _mockTokenBlacklistRepository.Verify(
                x => x.AddTokenBlacklistAsync(It.IsAny<TokenBlacklist>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AccessTokenSudahDiBlacklist_ShouldNotAddAgain()
        {
            SetupAuthenticatedUser();

            _mockTokenBlacklistRepository
                .Setup(x => x.IsBlacklistedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var logoutUserCommand = new LogoutUserCommand(RefreshToken: null, AccessToken: "already_blacklisted_token");
            var logoutUserResult = await _logoutUserHandler.Handle(logoutUserCommand);

            logoutUserResult.IsSuccess.Should().BeTrue();
            _mockTokenBlacklistRepository.Verify(
                x => x.AddTokenBlacklistAsync(It.IsAny<TokenBlacklist>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
