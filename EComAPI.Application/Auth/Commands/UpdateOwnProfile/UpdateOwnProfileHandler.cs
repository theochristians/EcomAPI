using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Auth.ValueObjects;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.UpdateOwnProfile
{
    public class UpdateOwnProfileHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOwnProfileHandler(
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UpdateOwnProfileCommand updateOwnProfileCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                var user = await _userRepository.GetUserByIdAsync(_currentUser.UserId, cancellationToken);
                if (user == null)
                    return Result<Guid>.Failure("User not found");

                var fullName = updateOwnProfileCommand.FullName ?? user.FullName;
                var phone = updateOwnProfileCommand.Phone ?? user.Phone;
                var avatar = updateOwnProfileCommand.Avatar ?? user.Avatar;
                var dateOfBirth = updateOwnProfileCommand.DateOfBirth ?? user.DateOfBirth;
                var gender = updateOwnProfileCommand.Gender ?? user.Gender;

                user.UpdateProfile(
                    fullName,
                    phone,
                    _currentUser.UserId,
                    avatar,
                    dateOfBirth,
                    gender
                );

                var requestedEmail = updateOwnProfileCommand.Email?.Trim();
                if (!string.IsNullOrWhiteSpace(requestedEmail))
                {
                    var normalizedRequestedEmail = requestedEmail.ToLowerInvariant();
                    var normalizedCurrentEmail = user.Email.Value.ToLowerInvariant();

                    if (normalizedRequestedEmail != normalizedCurrentEmail)
                    {
                        var newEmail = EmailAddress.Create(requestedEmail);

                        var emailExists = await _userRepository.ExistsUserAsync(newEmail.Value, cancellationToken);
                        if (emailExists)
                            return Result<Guid>.Failure("Email is already in use");

                        user.UpdateEmail(newEmail, _currentUser.UserId);
                    }
                }

                await _userRepository.UpdateUserAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(user.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to update profile: {exception.Message}");
            }
        }
    }
}
