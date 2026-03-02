using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Auth.Commands.UpdateOwnProfile;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;

namespace EComAPI.Application.Tests.Auth.Commands.UpdateOwnProfile
{
    public class UpdateOwnProfileHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UpdateOwnProfileHandler _updateOwnProfileHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public UpdateOwnProfileHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _updateOwnProfileHandler = new UpdateOwnProfileHandler(
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

        private User CreateUser() => new User(
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

            var updateOwnProfileResult = await _updateOwnProfileHandler.Handle(new UpdateOwnProfileCommand(null, null, null, null, null, null));

            updateOwnProfileResult.IsSuccess.Should().BeFalse();
            updateOwnProfileResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_UserTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var updateOwnProfileResult = await _updateOwnProfileHandler.Handle(new UpdateOwnProfileCommand(null, null, null, null, null, null));

            updateOwnProfileResult.IsSuccess.Should().BeFalse();
            updateOwnProfileResult.Error.Should().Be("User not found");
        }

        [Fact]
        public async Task Handle_UpdateEmail_EmailSudahAda_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var user = CreateUser();

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockUserRepository
                .Setup(x => x.ExistsUserAsync("newemail@example.com", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var updateOwnProfileCommand = new UpdateOwnProfileCommand(
                FullName: null, Email: "newemail@example.com", Phone: null,
                Avatar: null, DateOfBirth: null, Gender: null);

            var updateOwnProfileResult = await _updateOwnProfileHandler.Handle(updateOwnProfileCommand);

            updateOwnProfileResult.IsSuccess.Should().BeFalse();
            updateOwnProfileResult.Error.Should().Be("Email is already in use");
        }

        [Fact]
        public async Task Handle_UpdateNamaSaja_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();
            var user = CreateUser();

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var updateOwnProfileCommand = new UpdateOwnProfileCommand(
                FullName: "Jane Doe", Email: null, Phone: null,
                Avatar: null, DateOfBirth: null, Gender: null);

            var updateOwnProfileResult = await _updateOwnProfileHandler.Handle(updateOwnProfileCommand);

            updateOwnProfileResult.IsSuccess.Should().BeTrue();
            updateOwnProfileResult.Value.Should().Be(user.Id);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
