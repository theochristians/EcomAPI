using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Commands.ReturnCommands.RejectReturn
{
    public class RejectReturnHandler
    {
        private readonly IReturnRepository _returnRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RejectReturnHandler(
            IReturnRepository returnRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _returnRepository = returnRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(
            RejectReturnCommand rejectReturnCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<bool>.Failure("User not authenticated");

                var returnEntity = await _returnRepository.GetReturnByIdAsync(rejectReturnCommand.ReturnId, cancellationToken);
                if (returnEntity == null)
                    return Result<bool>.Failure("Return not found");

                // Reject the return (domain validates status transition)
                returnEntity.Reject(_currentUser.UserId);

                await _returnRepository.UpdateReturnAsync(returnEntity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<bool>.Success(true);
            }
            catch (DomainException domainException)
            {
                return Result<bool>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<bool>.Failure("An error occurred while rejecting the return");
            }
        }
    }
}
