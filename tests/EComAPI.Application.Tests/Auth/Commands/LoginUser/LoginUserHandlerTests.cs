using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Auth.Commands.LoginUser;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;

namespace EComAPI.Application.Tests.Auth.Commands.LoginUser
{
    public class LoginUserHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ILoginHistoryRepository> _mockLoginHistoryRepository;
        private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly LoginUserHandler _loginUserHandler;

        public LoginUserHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLoginHistoryRepository = new Mock<ILoginHistoryRepository>();
            _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _loginUserHandler = new LoginUserHandler(
                _mockUserRepository.Object,
                _mockLoginHistoryRepository.Object,
                _mockRefreshTokenRepository.Object,
                _mockPasswordHasher.Object,
                _mockJwtTokenGenerator.Object,
                _mockUnitOfWork.Object
            );
        }

        [Fact]
        public async Task Handle_EmailKosong_ShouldReturnFailure()
        {
            var loginUserCommand = new LoginUserCommand(Email: "", Password: "password123");

            var loginUserResult = await _loginUserHandler.Handle(loginUserCommand);

            loginUserResult.IsSuccess.Should().BeFalse();
            loginUserResult.Error.Should().Be("Email is required");
        }

        [Fact]
        public async Task Handle_PasswordKosong_ShouldReturnFailure()
        {
            var loginUserCommand = new LoginUserCommand(Email: "john@example.com", Password: "");

            var loginUserResult = await _loginUserHandler.Handle(loginUserCommand);

            loginUserResult.IsSuccess.Should().BeFalse();
            loginUserResult.Error.Should().Be("Password is required");
        }

        [Fact]
        public async Task Handle_UserTidakDitemukan_ShouldReturnFailure()
        {
            var loginUserCommand = new LoginUserCommand(Email: "notfound@example.com", Password: "password123");

            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var loginUserResult = await _loginUserHandler.Handle(loginUserCommand);

            loginUserResult.IsSuccess.Should().BeFalse();
            loginUserResult.Error.Should().Be("Invalid credentials");
        }

        [Fact]
        public async Task Handle_EmailBelumTerverifikasi_ShouldReturnFailure()
        {
            var loginUserCommand = new LoginUserCommand(Email: "john@example.com", Password: "password123");

            var roleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new User(
                "John Doe",
                EmailAddress.Create("john@example.com"),
                PasswordHash.FromHash("hashedpass"),
                roleId,
                userId
            );

            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var loginUserResult = await _loginUserHandler.Handle(loginUserCommand);

            loginUserResult.IsSuccess.Should().BeFalse();
            loginUserResult.Error.Should().Contain("Email is not verified");
        }

        [Fact]
        public async Task Handle_PasswordSalah_ShouldReturnFailure()
        {
            var loginUserCommand = new LoginUserCommand(Email: "john@example.com", Password: "wrongpassword");

            var roleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new User(
                "John Doe",
                EmailAddress.Create("john@example.com"),
                PasswordHash.FromHash("hashedpass"),
                roleId,
                userId
            );
            user.VerifyEmail(userId);

            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(x => x.Verify("wrongpassword", "hashedpass"))
                .Returns(false);

            var loginUserResult = await _loginUserHandler.Handle(loginUserCommand);

            loginUserResult.IsSuccess.Should().BeFalse();
            loginUserResult.Error.Should().Be("Invalid credentials");
        }

        [Fact]
        public async Task Handle_ValidCredentials_ShouldReturnLoginDto()
        {
            var loginUserCommand = new LoginUserCommand(Email: "john@example.com", Password: "password123");

            var roleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var user = new User(
                "John Doe",
                EmailAddress.Create("john@example.com"),
                PasswordHash.FromHash("hashedpass"),
                roleId,
                userId
            );
            user.VerifyEmail(userId);

            _mockUserRepository
                .Setup(x => x.GetUserByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockPasswordHasher
                .Setup(x => x.Verify("password123", "hashedpass"))
                .Returns(true);

            _mockJwtTokenGenerator
                .Setup(x => x.GenerateTokenAsync(user))
                .ReturnsAsync("access_token_value");

            var loginUserResult = await _loginUserHandler.Handle(loginUserCommand);

            loginUserResult.IsSuccess.Should().BeTrue();
            loginUserResult.Value.Should().NotBeNull();
            loginUserResult.Value!.AccessToken.Should().Be("access_token_value");
            loginUserResult.Value.RefreshToken.Should().NotBeNullOrWhiteSpace();

            _mockRefreshTokenRepository.Verify(
                x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockLoginHistoryRepository.Verify(
                x => x.AddLoginHistoryAsync(It.IsAny<LoginHistory>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
