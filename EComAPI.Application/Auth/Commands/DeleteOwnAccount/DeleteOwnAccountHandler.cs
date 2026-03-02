using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Auth.Commands.DeleteOwnAccount
{
    public class DeleteOwnAccountHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteOwnAccountHandler(
            IUserRepository userRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            DeleteOwnAccountCommand deleteOwnAccountCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                var userById = await _userRepository.GetUserByIdAsync(_currentUser.UserId, cancellationToken);

                if (userById == null)
                    return Result<Guid>.Failure("User not found");

                var userId = userById.Id;

                await _userRepository.HardDeleteUserAsync(userById, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(userId);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to delete account: {exception.Message}");
            }
        }
    }
}
