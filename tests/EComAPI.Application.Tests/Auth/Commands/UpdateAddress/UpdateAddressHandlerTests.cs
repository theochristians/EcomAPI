using EComAPI.Application.Auth.Commands.UpdateAddress;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Auth.Commands.UpdateAddress
{
    public class UpdateAddressHandlerTests
    {
        private readonly Mock<IAddressRepository> _mockAddressRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UpdateAddressHandler _updateAddressHandler;
        private readonly Guid _userId = Guid.NewGuid();

        public UpdateAddressHandlerTests()
        {
            _mockAddressRepository = new Mock<IAddressRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _updateAddressHandler = new UpdateAddressHandler(
                _mockAddressRepository.Object,
                _mockCurrentUser.Object,
                _mockUnitOfWork.Object
            );
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private UpdateAddressCommand BuildValidCommand() => new UpdateAddressCommand(
            Id: Guid.NewGuid(),
            Label: "Rumah",
            RecipientName: "John Doe",
            RecipientPhone: "08123456789",
            FullAddress: "Jl. Sudirman No.1",
            City: "Jakarta",
            Province: "DKI Jakarta",
            PostalCode: "10110"
        );

        private Address BuildActiveAddress(Guid? userId = null) => new Address(
            userId: userId ?? _userId,
            label: "Rumah",
            recipientName: "John Doe",
            recipientPhone: "08123456789",
            fullAddress: "Jl. Sudirman No.1",
            city: "Jakarta",
            province: "DKI Jakarta",
            postalCode: "10110",
            isDefault: false,
            createdBy: _userId
        );

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var updateAddressResult = await _updateAddressHandler.Handle(BuildValidCommand());

            updateAddressResult.IsSuccess.Should().BeFalse();
            updateAddressResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_AddressTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockAddressRepository
                .Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Address?)null);

            var updateAddressResult = await _updateAddressHandler.Handle(BuildValidCommand());

            updateAddressResult.IsSuccess.Should().BeFalse();
            updateAddressResult.Error.Should().Be("Address not found");
        }

        [Fact]
        public async Task Handle_UserTidakPunyaAkses_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var addressMilikUserLain = BuildActiveAddress(userId: Guid.NewGuid());
            _mockAddressRepository
                .Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(addressMilikUserLain);

            var updateAddressResult = await _updateAddressHandler.Handle(BuildValidCommand());

            updateAddressResult.IsSuccess.Should().BeFalse();
            updateAddressResult.Error.Should().Be("You don't have permission to update this address");
        }

        [Fact]
        public async Task Handle_Valid_ShouldUpdateAddressAndReturnDto()
        {
            SetupAuthenticatedUser();
            var address = BuildActiveAddress();
            _mockAddressRepository
                .Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(address);

            var updateAddressResult = await _updateAddressHandler.Handle(BuildValidCommand());

            updateAddressResult.IsSuccess.Should().BeTrue();
            updateAddressResult.Value.Should().NotBeNull();
            updateAddressResult.Value!.Label.Should().Be("Rumah");
            updateAddressResult.Value.RecipientName.Should().Be("John Doe");
            _mockAddressRepository.Verify(
                x => x.UpdateAddressAsync(address, It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}