using EComAPI.Application.Auth.Commands.RefreshTokens;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Security;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Auth.Commands.RefreshTokens
{
    public class RefreshTokenHandlerTests
    {
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private readonly Mock<ITokenBlacklistRepository> _mockTokenBlacklistRepository;
        private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly RefreshTokenHandler _refreshTokenHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public RefreshTokenHandlerTests()
        {
            _mockRefreshTokenRepository    = new Mock<IRefreshTokenRepository>();
            _mockTokenBlacklistRepository  = new Mock<ITokenBlacklistRepository>();
            _mockJwtTokenGenerator         = new Mock<IJwtTokenGenerator>();
            _mockCurrentUser               = new Mock<ICurrentUser>();
            _mockUnitOfWork                = new Mock<IUnitOfWork>();

            _refreshTokenHandler = new RefreshTokenHandler(
                _mockRefreshTokenRepository.Object,
                _mockTokenBlacklistRepository.Object,
                _mockJwtTokenGenerator.Object,
                _mockCurrentUser.Object,
                _mockUnitOfWork.Object
            );
        }

        /// <summary>
        ///  Helper untuk membuat RefreshTokenCommand.
        /// </summary>
        private static RefreshTokenCommand BuildValidCommand(string token = "valid-refresh-token")
            => new RefreshTokenCommand(RefreshToken: token);

        /// Buat RefreshToken yang aktif (belum revoked, belum expired)
        private RefreshToken BuildActiveToken()
            => new RefreshToken(_userId, TokenHelper.HashToken("valid-refresh-token"), DateTime.UtcNow.AddDays(7), _userId);

        /// Buat RefreshToken yang sudah di-revoke
        private RefreshToken BuildRevokedToken()
        {
            var token = BuildActiveToken();
            token.Revoke("test revoke", _userId);
            return token;
        }

        /// Buat RefreshToken yang sudah expired (constructor validasi tgl, pakai reflection)
        private RefreshToken BuildExpiredToken()
        {
            var token = BuildActiveToken();
            typeof(RefreshToken)
                .GetProperty(nameof(RefreshToken.ExpiresAt))!
                .SetValue(token, DateTime.UtcNow.AddDays(-1));
            return token;
        }

        /// Pasang User ke navigation property RefreshToken (private setter, pakai reflection)
        private static void SetUser(RefreshToken token, User user)
            => typeof(RefreshToken)
                .GetProperty(nameof(RefreshToken.User))!
                .SetValue(token, user);

        private User BuildActiveUser()
            => new User("John Doe", EmailAddress.Create("john@test.com"), PasswordHash.FromHash("hash"), Guid.NewGuid(), _userId);

        private User BuildInactiveUser()
        {
            var user = BuildActiveUser();
            typeof(User)
                .GetProperty(nameof(User.IsActive))!
                .SetValue(user, false);
            return user;
        }

        [Fact]
        public async Task Handle_RefreshTokenKosong_ShouldReturnFailure()
        {
            var refreshTokenCommand = new RefreshTokenCommand(RefreshToken: "");

            var refreshTokenResult = await _refreshTokenHandler.Handle(refreshTokenCommand);

            refreshTokenResult.IsSuccess.Should().BeFalse();
            refreshTokenResult.Error.Should().Be("Refresh token is required");
        }

        [Fact]
        public async Task Handle_RefreshTokenTidakDitemukan_ShouldReturnFailure()
        {
            _mockRefreshTokenRepository
                .Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

            var refreshTokenResult = await _refreshTokenHandler.Handle(BuildValidCommand());

            refreshTokenResult.IsSuccess.Should().BeFalse();
            refreshTokenResult.Error.Should().Be("Invalid refresh token");
        }

        [Fact]
        public async Task Handle_RefreshTokenSudahRevoked_ShouldReturnFailure()
        {
            _mockRefreshTokenRepository
                .Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildRevokedToken());

            var refreshTokenResult = await _refreshTokenHandler.Handle(BuildValidCommand());

            refreshTokenResult.IsSuccess.Should().BeFalse();
            refreshTokenResult.Error.Should().Be("Refresh token has been revoked");
        }

        [Fact]
        public async Task Handle_RefreshTokenSudahExpired_ShouldReturnFailure()
        {
            _mockRefreshTokenRepository
                .Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildExpiredToken());

            var refreshTokenResult = await _refreshTokenHandler.Handle(BuildValidCommand());

            refreshTokenResult.IsSuccess.Should().BeFalse();
            refreshTokenResult.Error.Should().Be("Refresh token has expired");
        }

        [Fact]
        public async Task Handle_TokenMilikUserLain_ShouldReturnFailure()
        {
            var token = BuildActiveToken(); // punya _userId

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(Guid.NewGuid()); // user berbeda

            _mockRefreshTokenRepository
                .Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var refreshTokenResult = await _refreshTokenHandler.Handle(BuildValidCommand());

            refreshTokenResult.IsSuccess.Should().BeFalse();
            refreshTokenResult.Error.Should().Be("Refresh token does not belong to current user");
        }

        [Fact]
        public async Task Handle_UserTidakDitemukan_ShouldReturnFailure()
        {
            var token = BuildActiveToken(); // User navigation = null by default

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);
            _mockRefreshTokenRepository
                .Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var refreshTokenResult = await _refreshTokenHandler.Handle(BuildValidCommand());

            refreshTokenResult.IsSuccess.Should().BeFalse();
            refreshTokenResult.Error.Should().Be("User not found");
        }

        [Fact]
        public async Task Handle_AkunNonAktif_ShouldReturnFailure()
        {
            var token = BuildActiveToken();
            SetUser(token, BuildInactiveUser());

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);
            _mockRefreshTokenRepository
                .Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var refreshTokenResult = await _refreshTokenHandler.Handle(BuildValidCommand());

            refreshTokenResult.IsSuccess.Should().BeFalse();
            refreshTokenResult.Error.Should().Be("Account is deactivated");
        }

        [Fact]
        public async Task Handle_Valid_ShouldReturnTokenBaru()
        {
            var user = BuildActiveUser();
            var token = BuildActiveToken();
            SetUser(token, user);

            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);
            _mockRefreshTokenRepository
                .Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);
            _mockJwtTokenGenerator
                .Setup(x => x.GenerateTokenAsync(user))
                .ReturnsAsync("new_access_token");

            var refreshTokenResult = await _refreshTokenHandler.Handle(BuildValidCommand());

            refreshTokenResult.IsSuccess.Should().BeTrue();
            refreshTokenResult.Value!.AccessToken.Should().Be("new_access_token");
            refreshTokenResult.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();

            // Token lama harus di-revoke dan token baru disimpan
            _mockRefreshTokenRepository.Verify(x => x.UpdateRefreshTokenAsync(
                It.Is<RefreshToken>(t => t.IsRevoked), It.IsAny<CancellationToken>()), Times.Once);
            _mockRefreshTokenRepository.Verify(x => x.AddRefreshTokenAsync(
                It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
