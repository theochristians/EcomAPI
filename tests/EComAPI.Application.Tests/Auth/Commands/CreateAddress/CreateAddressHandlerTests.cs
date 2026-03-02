using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Auth.Commands.CreateAddress;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Tests.Auth.Commands.CreateAddress
{
    public class CreateAddressHandlerTests
    {
        private readonly Mock<IAddressRepository> _mockAddressRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CreateAddressHandler _createAddressHandler;

        private readonly Guid _userId = Guid.NewGuid();

        public CreateAddressHandlerTests()
        {
            _mockAddressRepository = new Mock<IAddressRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _createAddressHandler = new CreateAddressHandler(
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

        private CreateAddressCommand BuildValidCommand() => new CreateAddressCommand(
            Label: "Rumah",
            RecipientName: "John Doe",
            RecipientPhone: "08123456789",
            FullAddress: "Jl. Sudirman No.1",
            City: "Jakarta",
            Province: "DKI Jakarta",
            PostalCode: "10110",
            IsDefault: false
        );

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var createAddressResult = await _createAddressHandler.Handle(BuildValidCommand());

            createAddressResult.IsSuccess.Should().BeFalse();
            createAddressResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_AddressPertama_MustBeDefault()
        {
            SetupAuthenticatedUser();

            _mockAddressRepository
                .Setup(x => x.CountUserAddressesAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var createAddressCommand = BuildValidCommand() with { IsDefault = false };
            var createAddressResult = await _createAddressHandler.Handle(createAddressCommand);

            createAddressResult.IsSuccess.Should().BeTrue();
            _mockAddressRepository.Verify(
                x => x.AddAddressAsync(It.Is<Address>(a => a.IsDefault), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AddressBerikutnyaAsDefault_ShouldUnsetOtherDefaults()
        {
            SetupAuthenticatedUser();

            _mockAddressRepository
                .Setup(x => x.CountUserAddressesAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var createAddressCommand = BuildValidCommand() with { IsDefault = true };
            var createAddressResult = await _createAddressHandler.Handle(createAddressCommand);

            createAddressResult.IsSuccess.Should().BeTrue();
            _mockAddressRepository.Verify(
                x => x.UnsetAllAddressDefaultsAsync(_userId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidInput_ShouldReturnAddressDto()
        {
            SetupAuthenticatedUser();

            _mockAddressRepository
                .Setup(x => x.CountUserAddressesAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            var createAddressResult = await _createAddressHandler.Handle(BuildValidCommand());

            createAddressResult.IsSuccess.Should().BeTrue();
            createAddressResult.Value.Should().NotBeNull();
            createAddressResult.Value!.Label.Should().Be("Rumah");
            createAddressResult.Value.RecipientName.Should().Be("John Doe");

            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
