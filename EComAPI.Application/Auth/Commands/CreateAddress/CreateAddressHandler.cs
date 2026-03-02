using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.CreateAddress
{
    public class CreateAddressHandler
    {
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateAddressHandler(
            IAddressRepository addressRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AddressDto>> Handle(
            CreateAddressCommand createAddressCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // -------------------------
                // 1. AUTHENTICATION CHECK
                // -------------------------
                if (!_currentUser.IsAuthenticated)
                    return Result<AddressDto>.Failure("User not authenticated");

                // -------------------------
                // 2. DETERMINE IsDefault
                // Jika ini address pertama milik user → otomatis jadi default.
                // Jika sudah ada address lain → ikuti flag IsDefault dari request.
                // -------------------------
                var existingAddressCount = await _addressRepository.CountUserAddressesAsync(_currentUser.UserId, cancellationToken);
                var isFirstAddress = existingAddressCount == 0;
                var isDefault = isFirstAddress || createAddressCommand.IsDefault;

                // Unset existing defaults hanya jika address ini akan jadi default
                // dan bukan address pertama (tidak ada yang perlu di-unset)
                if (isDefault && !isFirstAddress)
                {
                    await _addressRepository.UnsetAllAddressDefaultsAsync(_currentUser.UserId, cancellationToken);
                }

                // -------------------------
                // 3. DOMAIN OPERATION
                // Domain akan memvalidasi ulang aturan bisnis (Guard checks).
                // Jika ada pelanggaran, DomainException akan ditangkap di catch bawah.
                // -------------------------
                var address = new Address(
                    _currentUser.UserId,
                    createAddressCommand.Label,
                    createAddressCommand.RecipientName,
                    createAddressCommand.RecipientPhone,
                    createAddressCommand.FullAddress,
                    createAddressCommand.City,
                    createAddressCommand.Province,
                    createAddressCommand.PostalCode,
                    isDefault,
                    _currentUser.UserId
                );

                // -------------------------
                // 4. PERSIST
                // -------------------------
                await _addressRepository.AddAddressAsync(address, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var addressDto = new AddressDto(
                    address.Id,
                    address.Label,
                    address.RecipientName,
                    address.RecipientPhone,
                    address.FullAddress,
                    address.City,
                    address.Province,
                    address.PostalCode,
                    address.IsDefault
                );

                return Result<AddressDto>.Success(addressDto);
            }
            catch (DomainException domainException)
            {
                // Ditangkap dari Domain Guard (validasi aturan bisnis)
                return Result<AddressDto>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                // Ditangkap dari error yang tidak terduga (DB error, dll)
                return Result<AddressDto>.Failure($"Failed to create address: {exception.Message}");
            }
        }
    }
}
