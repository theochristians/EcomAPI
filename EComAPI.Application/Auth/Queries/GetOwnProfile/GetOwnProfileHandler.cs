using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Queries.GetOwnProfile
{
    public class GetOwnProfileHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly ICurrentUser _currentUser;

        public GetOwnProfileHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IAddressRepository addressRepository,
            ICurrentUser currentUser)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _addressRepository = addressRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<UserProfileDto>> Handle(
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<UserProfileDto>.Failure("User not authenticated");

                var userById = await _userRepository.GetUserByIdAsync(_currentUser.UserId, cancellationToken);

                if (userById == null)
                    return Result<UserProfileDto>.Failure("User not found");

                var roleById = await _roleRepository.GetRoleByIdAsync(userById.RoleId, cancellationToken);

                var defaultAddress = await _addressRepository.GetDefaultAddressAsync(
                    userById.Id,
                    cancellationToken);

                var defaultAddressDto = defaultAddress != null ? new AddressDto(
                    defaultAddress.Id,
                    defaultAddress.Label,
                    defaultAddress.RecipientName,
                    defaultAddress.RecipientPhone,
                    defaultAddress.FullAddress,
                    defaultAddress.City,
                    defaultAddress.Province,
                    defaultAddress.PostalCode,
                    defaultAddress.IsDefault) : null;

                var userProfileDto = new UserProfileDto(
                    userById.Id,
                    userById.FullName,
                    userById.Email.Value,
                    userById.Phone,
                    userById.IsEmailVerified,
                    userById.IsActive,
                    roleById?.Name ?? "Unknown",
                    userById.Avatar,
                    userById.DateOfBirth,
                    userById.Gender,
                    userById.LastLoginAt,
                    defaultAddressDto,
                    userById.CreatedAt
                );

                return Result<UserProfileDto>.Success(userProfileDto);
            }
            catch (DomainException domainException)
            {
                return Result<UserProfileDto>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<UserProfileDto>.Failure($"Failed to get profile: {exception.Message}");
            }
        }
    }
}
