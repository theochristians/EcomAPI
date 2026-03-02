using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.DeleteAddress
{
    public class DeleteAddressHandler
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAddressHandler(
            IAddressRepository addressRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            DeleteAddressCommand deleteAddressCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                var addressById = await _addressRepository.GetAddressByIdAsync(deleteAddressCommand.Id, cancellationToken);

                if (addressById == null)
                    return Result<Guid>.Failure("Address not found");

                if (addressById.UserId != _currentUser.UserId)
                    return Result<Guid>.Failure("You don't have permission to delete this address");

                var wasDefault = addressById.IsDefault;

                addressById.Delete(_currentUser.UserId);

                await _addressRepository.UpdateAddressAsync(addressById, cancellationToken);

                // Jika address yang dihapus adalah default,
                // promosikan address lain yang masih aktif menjadi default baru.
                if (wasDefault)
                {
                    var remainingAddresses = await _addressRepository.GetUserAddressesAsync(_currentUser.UserId, cancellationToken);
                    var nextDefault = remainingAddresses.FirstOrDefault(a => a.Id != addressById.Id);

                    if (nextDefault != null)
                    {
                        nextDefault.SetAsDefault(_currentUser.UserId);
                        await _addressRepository.UpdateAddressAsync(nextDefault, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(addressById.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to delete address: {exception.Message}");
            }
        }
    }
}

