using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Auth.Queries.GetDefaultAddress;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Auth.Queries.GetDefaultAddress
{
    public class GetDefaultAddressHandlerTests
    {
        private readonly Mock<IAddressRepository> _mockAddressRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly GetDefaultAddressHandler _getDefaultAddressHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public GetDefaultAddressHandlerTests()
        {
            _mockAddressRepository = new Mock<IAddressRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();

            _getDefaultAddressHandler = new GetDefaultAddressHandler(
                _mockAddressRepository.Object,
                _mockCurrentUser.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private Address BuildDefaultAddress() => new Address(
            userId: _userId,
            label: "Rumah",
            recipientName: "John Doe",
            recipientPhone: "08123456789",
            fullAddress: "Jl. Sudirman No.1",
            city: "Jakarta",
            province: "DKI Jakarta",
            postalCode: "10110",
            isDefault: true,
            createdBy: _userId
        );

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var getDefaultAddressResult = await _getDefaultAddressHandler.Handle();

            getDefaultAddressResult.IsSuccess.Should().BeFalse();
            getDefaultAddressResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_TidakAdaDefaultAddress_ShouldReturnSuccessWithNull()
        {
            SetupAuthenticatedUser();

            _mockAddressRepository
                .Setup(x => x.GetDefaultAddressAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Address?)null);

            var getDefaultAddressResult = await _getDefaultAddressHandler.Handle();

            getDefaultAddressResult.IsSuccess.Should().BeTrue();
            getDefaultAddressResult.Value.Should().BeNull();
        }

        [Fact]
        public async Task Handle_AdaDefaultAddress_ShouldReturnDefaultAddress()
        {
            SetupAuthenticatedUser();

            var defaultAddress = BuildDefaultAddress();

            _mockAddressRepository
                .Setup(x => x.GetDefaultAddressAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(defaultAddress);

            var getDefaultAddressResult = await _getDefaultAddressHandler.Handle();

            getDefaultAddressResult.IsSuccess.Should().BeTrue();
            getDefaultAddressResult.Value.Should().NotBeNull();
            getDefaultAddressResult.Value!.Id.Should().Be(defaultAddress.Id);
            getDefaultAddressResult.Value.Label.Should().Be(defaultAddress.Label);
            getDefaultAddressResult.Value.IsDefault.Should().BeTrue();
        }
    }
}