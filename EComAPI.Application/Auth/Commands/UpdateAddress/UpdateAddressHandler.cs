using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.UpdateAddress
{
    public class UpdateAddressHandler
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAddressHandler(
            IAddressRepository addressRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AddressDto>> Handle(
            UpdateAddressCommand updateAddressCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<AddressDto>.Failure("User not authenticated");

                var addressById = await _addressRepository.GetAddressByIdAsync(updateAddressCommand.Id, cancellationToken);

                if (addressById == null)
                    return Result<AddressDto>.Failure("Address not found");

                if (addressById.UserId != _currentUser.UserId)
                    return Result<AddressDto>.Failure("You don't have permission to update this address");

                addressById.Update(
                    updateAddressCommand.Label,
                    updateAddressCommand.RecipientName,
                    updateAddressCommand.RecipientPhone,
                    updateAddressCommand.FullAddress,
                    updateAddressCommand.City,
                    updateAddressCommand.Province,
                    updateAddressCommand.PostalCode,
                    _currentUser.UserId
                );

                await _addressRepository.UpdateAddressAsync(addressById, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var addressDto = new AddressDto(
                    addressById.Id,
                    addressById.Label,
                    addressById.RecipientName,
                    addressById.RecipientPhone,
                    addressById.FullAddress,
                    addressById.City,
                    addressById.Province,
                    addressById.PostalCode,
                    addressById.IsDefault
                );

                return Result<AddressDto>.Success(addressDto);
            }
            catch (DomainException domainException)
            {
                return Result<AddressDto>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<AddressDto>.Failure($"Failed to update address: {exception.Message}");
            }
        }
    }
}

