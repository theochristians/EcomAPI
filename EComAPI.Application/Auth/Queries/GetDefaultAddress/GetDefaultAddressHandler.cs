using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Queries.GetDefaultAddress
{
    public class GetDefaultAddressHandler
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUser _currentUser;

        public GetDefaultAddressHandler(
            IAddressRepository addressRepository,
            ICurrentUser currentUser)
        {
            _addressRepository = addressRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<AddressDto?>> Handle(
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<AddressDto?>.Failure("User not authenticated");

                var defaultAddress = await _addressRepository.GetDefaultAddressAsync(
                    _currentUser.UserId,
                    cancellationToken);

                if (defaultAddress == null)
                    return Result<AddressDto?>.Success(null);

                var addressDto = new AddressDto(
                    defaultAddress.Id,
                    defaultAddress.Label,
                    defaultAddress.RecipientName,
                    defaultAddress.RecipientPhone,
                    defaultAddress.FullAddress,
                    defaultAddress.City,
                    defaultAddress.Province,
                    defaultAddress.PostalCode,
                    defaultAddress.IsDefault);

                return Result<AddressDto?>.Success(addressDto);

            }
            catch (DomainException domainException)
            {
                return Result<AddressDto?>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<AddressDto?>.Failure($"Failed to get default address: {exception.Message}");
            }
        }
    }
}

