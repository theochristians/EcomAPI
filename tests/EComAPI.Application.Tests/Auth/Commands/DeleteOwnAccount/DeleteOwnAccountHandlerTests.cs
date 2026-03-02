using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Auth.Commands.DeleteOwnAccount;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;

namespace EComAPI.Application.Tests.Auth.Commands.DeleteOwnAccount
{
    public class DeleteOwnAccountHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly DeleteOwnAccountHandler _deleteOwnAccountHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public DeleteOwnAccountHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _deleteOwnAccountHandler = new DeleteOwnAccountHandler(
                _mockUserRepository.Object,
                _mockCurrentUser.Object,
                _mockUnitOfWork.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private User BuildActiveUser() => new User(
            "John Doe",
            EmailAddress.Create("john@example.com"),
            PasswordHash.FromHash("hashedpass"),
            Guid.NewGuid(),
            _userId
        );

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var deleteOwnAccountResult = await _deleteOwnAccountHandler.Handle(new DeleteOwnAccountCommand());

            deleteOwnAccountResult.IsSuccess.Should().BeFalse();
            deleteOwnAccountResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_UserTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var deleteOwnAccountResult = await _deleteOwnAccountHandler.Handle(new DeleteOwnAccountCommand());

            deleteOwnAccountResult.IsSuccess.Should().BeFalse();
            deleteOwnAccountResult.Error.Should().Be("User not found");
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldHardDeleteAndReturnUserId()
        {
            SetupAuthenticatedUser();
            var user = BuildActiveUser();

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var deleteOwnAccountResult = await _deleteOwnAccountHandler.Handle(new DeleteOwnAccountCommand());

            deleteOwnAccountResult.IsSuccess.Should().BeTrue();
            _mockUserRepository.Verify(
                x => x.HardDeleteUserAsync(user, It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
