using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Commands.ReviewCommands.UpdateReview
{
    public class UpdateReviewHandler
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateReviewHandler(
            IReviewRepository reviewRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            UpdateReviewCommand updateReviewCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                var review = await _reviewRepository.GetReviewByIdAsync(
                    updateReviewCommand.ReviewId, cancellationToken);

                if (review == null)
                    return Result<Guid>.Failure("Review not found");

                if (review.UserId != _currentUser.UserId)
                    return Result<Guid>.Failure("Review not found");

                review.Update(
                    updateReviewCommand.Rating,
                    updateReviewCommand.Comment,
                    _currentUser.UserId);

                await _reviewRepository.UpdateReviewAsync(review, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(review.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<Guid>.Failure("An error occurred while updating the review");
            }
        }
    }
}
