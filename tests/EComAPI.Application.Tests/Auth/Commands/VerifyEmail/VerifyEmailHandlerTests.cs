using EComAPI.Application.Auth.Commands.VerifyEmail;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Auth.Commands.VerifyEmail
{
    public class VerifyEmailHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IEmailVerificationRepository> _mockEmailVerificationRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly VerifyEmailHandler _handler;

        private readonly Guid _userId = Guid.NewGuid();

        public VerifyEmailHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockEmailVerificationRepository = new Mock<IEmailVerificationRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _handler = new VerifyEmailHandler(
                _mockUserRepository.Object,
                _mockEmailVerificationRepository.Object,
                _mockCurrentUser.Object,
                _mockUnitOfWork.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private User BuildUnverifiedUser()
        {
            var email = EmailAddress.Create($"user_{Guid.NewGuid():N}@test.com");
            var password = PasswordHash.FromHash("hashed_password");
            return new User("Test User", email, password, Guid.NewGuid(), _userId);
        }

        private User BuildVerifiedUser()
        {
            var user = BuildUnverifiedUser();
            user.VerifyEmail(_userId);
            return user;
        }

        private EmailVerification BuildValidVerification(Guid userId, string code = "ABC123")
            => new EmailVerification(userId, code, DateTime.UtcNow.AddMinutes(10), _userId);

        private EmailVerification BuildExpiredVerification(Guid userId, string code = "ABC123")
        {
            var ev = new EmailVerification(userId, code, DateTime.UtcNow.AddMinutes(10), _userId);
            // Bypass private setter menggunakan reflection untuk simulasi expiry
            typeof(EmailVerification)
                .GetProperty("ExpiresAt")!
                .SetValue(ev, DateTime.UtcNow.AddMinutes(-5));
            return ev;
        }

        private EmailVerification BuildMaxAttemptsVerification(Guid userId, string code = "ABC123")
        {
            var ev = new EmailVerification(userId, code, DateTime.UtcNow.AddMinutes(10), _userId);
            for (int i = 0; i < 5; i++)
                ev.IncrementAttempt(_userId);
            return ev;
        }

        // ─────────────────────────────────────────────────────────
        // Input validation
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _handler.Handle(new VerifyEmailCommand("ABC123"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_KodeKosong_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _handler.Handle(new VerifyEmailCommand(""));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Verification code is required");
        }

        [Fact]
        public async Task Handle_KodeBukanEnamKarakter_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var result = await _handler.Handle(new VerifyEmailCommand("12345")); // 5 karakter

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Verification code must be 6 characters");
        }

        // ─────────────────────────────────────────────────────────
        // User state checks
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_UserTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var result = await _handler.Handle(new VerifyEmailCommand("ABC123"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not found");
        }

        [Fact]
        public async Task Handle_EmailSudahVerified_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var verifiedUser = BuildVerifiedUser();
            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(verifiedUser);

            var result = await _handler.Handle(new VerifyEmailCommand("ABC123"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Email is already verified");
        }

        // ─────────────────────────────────────────────────────────
        // Verification record checks
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_TidakAdaPendingVerification_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var user = BuildUnverifiedUser();
            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockEmailVerificationRepository
                .Setup(x => x.GetLatestPendingByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EmailVerification?)null);

            var result = await _handler.Handle(new VerifyEmailCommand("ABC123"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Verification code not found");
        }

        [Fact]
        public async Task Handle_KodeExpired_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var user = BuildUnverifiedUser();
            var expiredVerification = BuildExpiredVerification(user.Id);

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockEmailVerificationRepository
                .Setup(x => x.GetLatestPendingByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expiredVerification);

            var result = await _handler.Handle(new VerifyEmailCommand("ABC123"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("expired");
        }

        [Fact]
        public async Task Handle_MaxAttemptsReached_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var user = BuildUnverifiedUser();
            var maxedVerification = BuildMaxAttemptsVerification(user.Id);

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockEmailVerificationRepository
                .Setup(x => x.GetLatestPendingByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(maxedVerification);

            var result = await _handler.Handle(new VerifyEmailCommand("ABC123"));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("Maximum verification attempts");
        }

        // ─────────────────────────────────────────────────────────
        // Code matching
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_KodeSalah_ShouldIncrementAttemptDanReturnFailure()
        {
            SetupAuthenticatedUser();
            var user = BuildUnverifiedUser();
            var verification = BuildValidVerification(user.Id, "ABC123");
            var initialAttempts = verification.Attempts;

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockEmailVerificationRepository
                .Setup(x => x.GetLatestPendingByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(verification);

            var result = await _handler.Handle(new VerifyEmailCommand("WRONG1")); // kode salah

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Invalid verification code");

            // Attempts harus bertambah 1
            verification.Attempts.Should().Be(initialAttempts + 1);

            // UpdateEmailVerification dipanggil untuk menyimpan penambahan attempt
            _mockEmailVerificationRepository.Verify(
                x => x.UpdateEmailVerificationAsync(verification, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ─────────────────────────────────────────────────────────
        // Happy path
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_KodeBenar_ShouldVerifyEmailDanReturnSuccess()
        {
            SetupAuthenticatedUser();
            var user = BuildUnverifiedUser();
            var verification = BuildValidVerification(user.Id, "ABC123");

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockEmailVerificationRepository
                .Setup(x => x.GetLatestPendingByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(verification);

            var result = await _handler.Handle(new VerifyEmailCommand("ABC123"));

            result.IsSuccess.Should().BeTrue();

            // User harus ter-mark sebagai verified
            user.IsEmailVerified.Should().BeTrue();

            // Verification record harus di-mark verified
            verification.IsVerified.Should().BeTrue();

            // Semua persisten dipanggil
            _mockEmailVerificationRepository.Verify(
                x => x.UpdateEmailVerificationAsync(verification, It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUserRepository.Verify(
                x => x.UpdateUserAsync(user, It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
