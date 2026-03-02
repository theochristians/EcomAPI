using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Auth.Queries.GetOwnProfile;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Auth.ValueObjects;

namespace EComAPI.Application.Tests.Auth.Queries.GetOwnProfile
{
    public class GetOwnProfileHandlerTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IRoleRepository> _mockRoleRepository;
        private readonly Mock<IAddressRepository> _mockAddressRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly GetOwnProfileHandler _getOwnProfileHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public GetOwnProfileHandlerTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockRoleRepository = new Mock<IRoleRepository>();
            _mockAddressRepository = new Mock<IAddressRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();

            _getOwnProfileHandler = new GetOwnProfileHandler(
                _mockUserRepository.Object,
                _mockRoleRepository.Object,
                _mockAddressRepository.Object,
                _mockCurrentUser.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private User CreateUser()
        {
            var roleId = Guid.NewGuid();
            return new User(
                "John Doe",
                EmailAddress.Create("john@example.com"),
                PasswordHash.FromHash("hashedpass"),
                roleId,
                _userId
            );
        }

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var getOwnProfileResult = await _getOwnProfileHandler.Handle();

            getOwnProfileResult.IsSuccess.Should().BeFalse();
            getOwnProfileResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_UserTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var getOwnProfileResult = await _getOwnProfileHandler.Handle();

            getOwnProfileResult.IsSuccess.Should().BeFalse();
            getOwnProfileResult.Error.Should().Be("User not found");
        }

        [Fact]
        public async Task Handle_UserDitemukan_ShouldReturnProfile()
        {
            SetupAuthenticatedUser();
            var user = CreateUser();
            var role = new Role("Customer", Guid.NewGuid());

            _mockUserRepository
                .Setup(x => x.GetUserByIdAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            _mockRoleRepository
                .Setup(x => x.GetRoleByIdAsync(user.RoleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(role);

            _mockAddressRepository
                .Setup(x => x.GetDefaultAddressAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Address?)null);

            var getOwnProfileResult = await _getOwnProfileHandler.Handle();

            getOwnProfileResult.IsSuccess.Should().BeTrue();
            getOwnProfileResult.Value.Should().NotBeNull();
            getOwnProfileResult.Value!.FullName.Should().Be("John Doe");
            getOwnProfileResult.Value.Email.Should().Be("john@example.com");
            getOwnProfileResult.Value.RoleName.Should().Be("Customer");
        }
    }
}
