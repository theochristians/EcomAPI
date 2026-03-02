using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Auth.Queries.GetUserAddresses;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Auth.Queries.GetUserAddresses
{
    public class GetUserAddressesHandlerTests
    {
        private readonly Mock<IAddressRepository> _mockAddressRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly GetUserAddressesHandler _getUserAddressesHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public GetUserAddressesHandlerTests()
        {
            _mockAddressRepository = new Mock<IAddressRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();

            _getUserAddressesHandler = new GetUserAddressesHandler(
                _mockAddressRepository.Object,
                _mockCurrentUser.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private List<Address> BuildUserAddresses() => new List<Address>
        {
            new Address(
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
            ),
            new Address(
                userId: _userId,
                label: "Kantor",
                recipientName: "John Doe",
                recipientPhone: "08123456789",
                fullAddress: "Jl. Thamrin No.1",
                city: "Jakarta",
                province: "DKI Jakarta",
                postalCode: "10210",
                isDefault: false,
                createdBy: _userId
            )
        };

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var getUserAddressesResult = await _getUserAddressesHandler.Handle();

            getUserAddressesResult.IsSuccess.Should().BeFalse();
            getUserAddressesResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_AddressKosong_ShouldReturnEmptyList()
        {
            SetupAuthenticatedUser();

            _mockAddressRepository
                .Setup(x => x.GetUserAddressesAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Address>());

            var getUserAddressesResult = await _getUserAddressesHandler.Handle();

            getUserAddressesResult.IsSuccess.Should().BeTrue();
            getUserAddressesResult.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_AddressDitemukan_ShouldReturnListOfAddresses()
        {
            SetupAuthenticatedUser();

            var userAddresses = BuildUserAddresses();

            _mockAddressRepository
                .Setup(x => x.GetUserAddressesAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userAddresses);

            var getUserAddressesResult = await _getUserAddressesHandler.Handle();

            getUserAddressesResult.IsSuccess.Should().BeTrue();
            getUserAddressesResult.Value.Should().HaveCount(2);
            getUserAddressesResult.Value.Should().ContainSingle(a => a.Label == "Rumah" && a.IsDefault);
            getUserAddressesResult.Value.Should().ContainSingle(a => a.Label == "Kantor" && !a.IsDefault);
        }
    }
}