using EComAPI.Application.Auth.Commands.SendEmailVerification;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Auth.Commands.SendEmailVerification
{
    public class SendEmailVerificationHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IEmailVerificationRepository> _mockEmailVerificationRepository;
        private readonly Mock<IEmailSender> _mockEmailSender;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly SendEmailVerificationHandler _handler;

        private readonly Guid _userId = Guid.NewGuid();

        public SendEmailVerificationHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockEmailVerificationRepository = new Mock<IEmailVerificationRepository>();
            _mockEmailSender = new Mock<IEmailSender>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _handler = new SendEmailVerificationHandler(
                _mockUserRepository.Object,
                _mockEmailVerificationRepository.Object,
                _mockEmailSender.Object,
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

        private EmailVerification BuildPendingVerification(Guid userId)
            => new EmailVerification(userId, "ABC123", DateTime.UtcNow.AddMinutes(10), _userId);

        // ─────────────────────────────────────────────────────────
        // Authentication
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var result = await _handler.Handle(new SendEmailVerificationCommand());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        // ─────────────────────────────────────────────────────────
        // User not found
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_UserTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var result = await _handler.Handle(new SendEmailVerificationCommand());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not found");
        }

        // ─────────────────────────────────────────────────────────
        // Already verified
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EmailSudahVerified_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var verifiedUser = BuildVerifiedUser();
            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(verifiedUser);

            var result = await _handler.Handle(new SendEmailVerificationCommand());

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Email is already verified");
        }

        // ─────────────────────────────────────────────────────────
        // Happy path — tidak ada pending sebelumnya
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_TidakAdaPending_ShouldKirimCodeDanReturnExpiry()
        {
            SetupAuthenticatedUser();
            var user = BuildUnverifiedUser();
            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockEmailVerificationRepository
                .Setup(x => x.GetLatestPendingByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EmailVerification?)null);

            var result = await _handler.Handle(new SendEmailVerificationCommand());

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeAfter(DateTime.UtcNow);

            // OTP baru harus disimpan
            _mockEmailVerificationRepository.Verify(
                x => x.AddEmailVerificationAsync(It.IsAny<EmailVerification>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Email harus dikirim
            _mockEmailSender.Verify(
                x => x.SendEmailVerificationCodeAsync(
                    user.Email.Value,
                    user.FullName,
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // ─────────────────────────────────────────────────────────
        // Happy path — ada pending sebelumnya → di-invalidate dulu
        // ─────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_AdaPendingSebelumnya_ShouldInvalidateOldDanKirimBaru()
        {
            SetupAuthenticatedUser();
            var user = BuildUnverifiedUser();
            var oldPending = BuildPendingVerification(user.Id);

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            _mockEmailVerificationRepository
                .Setup(x => x.GetLatestPendingByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(oldPending);

            var result = await _handler.Handle(new SendEmailVerificationCommand());

            result.IsSuccess.Should().BeTrue();

            // OTP lama harus di-update (invalidate)
            _mockEmailVerificationRepository.Verify(
                x => x.UpdateEmailVerificationAsync(oldPending, It.IsAny<CancellationToken>()),
                Times.Once);

            // OTP baru harus ditambah
            _mockEmailVerificationRepository.Verify(
                x => x.AddEmailVerificationAsync(It.IsAny<EmailVerification>(), It.IsAny<CancellationToken>()),
                Times.Once);

            // Email tetap dikirim
            _mockEmailSender.Verify(
                x => x.SendEmailVerificationCodeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
