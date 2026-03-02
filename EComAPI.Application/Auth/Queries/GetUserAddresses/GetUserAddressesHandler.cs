using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Queries.GetUserAddresses
{
    public class GetUserAddressesHandler
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUser _currentUser;

        public GetUserAddressesHandler(
            IAddressRepository addressRepository,
            ICurrentUser currentUser)
        {
            _addressRepository = addressRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<AddressDto>>> Handle(
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<IReadOnlyList<AddressDto>>.Failure("User not authenticated");

                var userAddresses = await _addressRepository.GetUserAddressesAsync(
                    _currentUser.UserId,
                    cancellationToken);

                var addressDtos = userAddresses.Select(address => new AddressDto(
                    address.Id,
                    address.Label,
                    address.RecipientName,
                    address.RecipientPhone,
                    address.FullAddress,
                    address.City,
                    address.Province,
                    address.PostalCode,
                    address.IsDefault
                )).ToList();

                return Result<IReadOnlyList<AddressDto>>.Success(addressDtos);
            }
            catch (DomainException domainException)
            {
                return Result<IReadOnlyList<AddressDto>>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<IReadOnlyList<AddressDto>>.Failure($"Failed to get user addresses: {exception.Message}");
            }
        }
    }
}

