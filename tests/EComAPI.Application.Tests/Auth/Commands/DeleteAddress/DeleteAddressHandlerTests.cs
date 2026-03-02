using EComAPI.Application.Auth.Commands.DeleteAddress;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Domain.Auth.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EComAPI.Application.Tests.Auth.Commands.DeleteAddress
{
    public class DeleteAddressHandlerTests
    {
        private readonly Mock<IAddressRepository> _mockAddressRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly DeleteAddressHandler _deleteAddressHandler;
        private readonly Guid _userId = Guid.NewGuid();

        public DeleteAddressHandlerTests()
        {
            _mockAddressRepository = new Mock<IAddressRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _deleteAddressHandler = new DeleteAddressHandler(
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
        private DeleteAddressCommand BuildValidCommand() => new DeleteAddressCommand(
            Id: Guid.NewGuid()
        );

        [Fact]
        public async Task Handle_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var deleteAddressResult = await _deleteAddressHandler.Handle(BuildValidCommand());

            deleteAddressResult.IsSuccess.Should().BeFalse();
            deleteAddressResult.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task Handle_AddressTidakDitemukan_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            _mockAddressRepository.Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Address?)null);

            var deleteAddressResult = await _deleteAddressHandler.Handle(BuildValidCommand());

            deleteAddressResult.IsSuccess.Should().BeFalse();
            deleteAddressResult.Error.Should().Be("Address not found");
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

            var deleteAddressResult = await _deleteAddressHandler.Handle(BuildValidCommand());

            deleteAddressResult.IsSuccess.Should().BeFalse();
            deleteAddressResult.Error.Should().Be("You don't have permission to delete this address");
        }

        [Fact]
        public async Task Handle_HapusAddressNonDefault_ShouldDeleteTanpaPromosi()
        {
            SetupAuthenticatedUser();
            var address = new Address(
                userId: _userId,
                label: "Kantor",
                recipientName: "John Doe",
                recipientPhone: "08123456789",
                fullAddress: "Jl. Sudirman No.1",
                city: "Jakarta",
                province: "DKI Jakarta",
                postalCode: "10110",
                isDefault: false, // ← bukan default, tidak trigger promosi
                createdBy: _userId
            );
            _mockAddressRepository.Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(address);

            var deleteAddressResult = await _deleteAddressHandler.Handle(BuildValidCommand());

            deleteAddressResult.IsSuccess.Should().BeTrue();
            deleteAddressResult.Value.Should().Be(address.Id);

            _mockAddressRepository.Verify(x => x.UpdateAddressAsync(It.Is<Address>(a => a.Id == address.Id && a.IsDeleted), It.IsAny<CancellationToken>()), Times.Once);
            // GetUserAddressesAsync tidak boleh dipanggil karena bukan default
            _mockAddressRepository.Verify(x => x.GetUserAddressesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_HapusDefaultAddress_TidakAdaSisa_ShouldDeleteTanpaPromosi()
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
                isDefault: true, // ← default, tapi tidak ada address lain
                createdBy: _userId
            );
            _mockAddressRepository.Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(address);
            // Tidak ada address lain yang tersisa
            _mockAddressRepository.Setup(x => x.GetUserAddressesAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Address>());

            var deleteAddressResult = await _deleteAddressHandler.Handle(BuildValidCommand());

            deleteAddressResult.IsSuccess.Should().BeTrue();
            deleteAddressResult.Value.Should().Be(address.Id);
            _mockAddressRepository.Verify(x => x.UpdateAddressAsync(It.Is<Address>(a => a.Id == address.Id && a.IsDeleted), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_HapusDefaultAddress_AdaAddressLain_ShouldPromosiAddressBerikutnya()
        {
            SetupAuthenticatedUser();
            var defaultAddress = new Address(
                userId: _userId,
                label: "Rumah",
                recipientName: "John Doe",
                recipientPhone: "08123456789",
                fullAddress: "Jl. Sudirman No.1",
                city: "Jakarta",
                province: "DKI Jakarta",
                postalCode: "10110",
                isDefault: true, // ← ini yang dihapus
                createdBy: _userId
            );
            var otherAddress = new Address(
                userId: _userId,
                label: "Kantor",
                recipientName: "John Doe",
                recipientPhone: "08123456789",
                fullAddress: "Jl. Thamrin No.2",
                city: "Jakarta",
                province: "DKI Jakarta",
                postalCode: "10220",
                isDefault: false, // ← ini yang akan dipromosi
                createdBy: _userId
            );
            _mockAddressRepository.Setup(x => x.GetAddressByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(defaultAddress);
            _mockAddressRepository.Setup(x => x.GetUserAddressesAsync(_userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Address> { defaultAddress, otherAddress });

            var deleteAddressResult = await _deleteAddressHandler.Handle(BuildValidCommand());

            deleteAddressResult.IsSuccess.Should().BeTrue();
            // UpdateAddressAsync dipanggil 2x: 1 untuk delete, 1 untuk promosi
            _mockAddressRepository.Verify(x => x.UpdateAddressAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            // otherAddress harus jadi default setelah promosi
            otherAddress.IsDefault.Should().BeTrue();
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}