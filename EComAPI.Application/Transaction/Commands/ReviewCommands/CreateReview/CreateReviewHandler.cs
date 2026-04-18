using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Commands.ReviewCommands.CreateReview
{
    public class CreateReviewHandler
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateReviewHandler(
            IReviewRepository reviewRepository,
            IOrderRepository orderRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _orderRepository = orderRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateReviewCommand createReviewCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (createReviewCommand.OrderId == Guid.Empty)
                    return Result<Guid>.Failure("OrderId is required");

                if (createReviewCommand.ProductId == Guid.Empty)
                    return Result<Guid>.Failure("ProductId is required");

                // Check if user already reviewed this product
                var alreadyReviewed = await _reviewRepository.UserHasReviewedOrderProductAsync(
                    _currentUser.UserId,
                    createReviewCommand.OrderId,
                    createReviewCommand.ProductId,
                    cancellationToken);

                if (alreadyReviewed)
                    return Result<Guid>.Failure("You have already reviewed this product");

                // Check if the specific order belongs to user, is completed, and contains the product.
                var hasCompletedOrder = await _orderRepository.UserHasCompletedOrderForProductInOrderAsync(
                    _currentUser.UserId,
                    createReviewCommand.OrderId,
                    createReviewCommand.ProductId,
                    cancellationToken);

                if (!hasCompletedOrder)
                    return Result<Guid>.Failure("You can only review products from your completed order");

                // Validate images count
                if (createReviewCommand.Images != null && createReviewCommand.Images.Count > 5)
                    return Result<Guid>.Failure("Maximum 5 images per review");

                // Create review
                var review = new Review(
                    userId: _currentUser.UserId,
                    orderId: createReviewCommand.OrderId,
                    productId: createReviewCommand.ProductId,
                    rating: createReviewCommand.Rating,
                    createdBy: _currentUser.UserId,
                    comment: createReviewCommand.Comment);

                await _reviewRepository.AddReviewAsync(review, cancellationToken);

                // Add images
                if (createReviewCommand.Images != null)
                {
                    foreach (var imageDto in createReviewCommand.Images)
                    {
                        var reviewImage = new ReviewImage(
                            reviewId: review.Id,
                            imageUrl: imageDto.ImageUrl,
                            displayOrder: imageDto.DisplayOrder);

                        review.AddImage(reviewImage);
                        await _reviewRepository.AddReviewImageAsync(reviewImage, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(review.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                if (exception.ToString().Contains("IX_Reviews_UserId_OrderId_ProductId", StringComparison.OrdinalIgnoreCase)
                    || exception.ToString().Contains("IX_Reviews_UserId_ProductId", StringComparison.OrdinalIgnoreCase))
                    return Result<Guid>.Failure("You have already reviewed this product");

                if (exception.ToString().Contains("OrderId", StringComparison.OrdinalIgnoreCase)
                    && exception.ToString().Contains("Invalid column name", StringComparison.OrdinalIgnoreCase))
                    return Result<Guid>.Failure("Database schema is outdated. Please run the latest migration first.");

                if (exception.ToString().Contains("FK_Reviews_Orders_OrderId", StringComparison.OrdinalIgnoreCase))
                    return Result<Guid>.Failure("Invalid OrderId. The selected order could not be found.");

                return Result<Guid>.Failure("An error occurred while creating the review");
            }
        }
    }
}
