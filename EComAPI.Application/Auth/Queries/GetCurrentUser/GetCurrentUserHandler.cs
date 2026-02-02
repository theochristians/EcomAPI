using EComAPI.Application.Auth.DTOs;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Results;

namespace EComAPI.Application.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserHandler
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _userRepository;

        public GetCurrentUserHandler(
            ICurrentUser currentUser,
            IUserRepository userRepository)
        {
            _currentUser = currentUser;
            _userRepository = userRepository;
        }

        public async Task<Result<UserProfileDto>> Handle(
            GetCurrentUserQuery query)
        {
            if (_currentUser.UserId is null)
                return Result<UserProfileDto>.Failure("Unauthorized");

            var user = await _userRepository.GetByIdAsync(_currentUser.UserId.Value);

            if (user is null)
                return Result<UserProfileDto>.Failure("User not found");

            return Result<UserProfileDto>.Success(
                new UserProfileDto(
                    user.Id,
                    user.FullName,
                    user.Email.Value,
                    user.IsEmailVerified
                )
            );
        }
    }
}