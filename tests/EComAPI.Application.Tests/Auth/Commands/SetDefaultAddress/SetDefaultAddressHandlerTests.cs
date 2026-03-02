using EComAPI.Application.Auth.Commands.SetDefaultAddress;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Auth.Commands.SetDefaultAddress
{
    public class SetDefaultAddressHandlerTests
    {
        private readonly Mock<IAddressRepository> _mockAddressRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly SetDefaultAddressHandler _setDefaultAddressHandler;
        private readonly Guid _userId = Guid.NewGuid();

        public SetDefaultAddressHandlerTests()
        {
            _mockAddressRepository = new Mock<IAddressRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _setDefaultAddressHandler = new SetDefaultAddressHandler(
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

        private SetDefaultAddressCommand BuildValidCommand() => new SetDefaultAddressCommand(
            Id: Guid.NewGuid()
        );

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var setDefaultAddressResult = await _setDefaultAddressHandler.Handle(BuildValidCommand());

            setDefaultAddressResult.IsSuccess.Should().BeFalse();
            setDefaultAddressResult.Error.Should().Be("User not authenticated");
        }
        
        [Fact]
        public async Task Handle_AddressTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockAddressRepository.Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Address?)null);

            var setDefaultAddressResult = await _setDefaultAddressHandler.Handle(BuildValidCommand());

            setDefaultAddressResult.IsSuccess.Should().BeFalse();
            setDefaultAddressResult.Error.Should().Be("Address not found");
        }

        [Fact]
        public async Task Handle_UserTidakPunyaAkses_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var address = new Address(
                userId: Guid.NewGuid(),
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
            _mockAddressRepository.Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(address);

            var setDefaultAddressResult = await _setDefaultAddressHandler.Handle(BuildValidCommand());

            setDefaultAddressResult.IsSuccess.Should().BeFalse();
            setDefaultAddressResult.Error.Should().Be("You don't have permission to modify this address");
        }

        [Fact]
        public async Task Handle_Valid_ShouldSetDefaultAddress()
        {
            SetupAuthenticatedUser();
            var address = new Address(
                userId: _userId,
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
            _mockAddressRepository.Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(address);

            var setDefaultAddressResult = await _setDefaultAddressHandler.Handle(BuildValidCommand());

            setDefaultAddressResult.IsSuccess.Should().BeTrue();
            setDefaultAddressResult.Value!.IsDefault.Should().BeTrue();
            _mockAddressRepository.Verify(
                x => x.UnsetAllAddressDefaultsAsync(_userId, It.IsAny<CancellationToken>()),
                Times.Once);
            _mockAddressRepository.Verify(
                x => x.UpdateAddressAsync(address, It.IsAny<CancellationToken>()),
                Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}