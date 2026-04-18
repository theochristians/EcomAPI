using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review?> GetReviewByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Review?> GetReviewWithImagesByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Review> Items, int TotalCount)> GetReviewsByProductIdAsync(
            Guid productId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<bool> UserHasReviewedOrderProductAsync(Guid userId, Guid orderId, Guid productId, CancellationToken cancellationToken = default);
        Task AddReviewAsync(Review review, CancellationToken cancellationToken = default);
        Task UpdateReviewAsync(Review review, CancellationToken cancellationToken = default);
        Task AddReviewImageAsync(ReviewImage reviewImage, CancellationToken cancellationToken = default);
        Task<ReviewImage?> GetReviewImageByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task RemoveReviewImageAsync(ReviewImage reviewImage, CancellationToken cancellationToken = default);
    }
}
