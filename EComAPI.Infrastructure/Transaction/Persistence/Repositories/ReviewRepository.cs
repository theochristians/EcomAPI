using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Transaction.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Transaction.Persistence.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _appDbContext;

        public ReviewRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Review?> GetReviewByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Reviews
                .FirstOrDefaultAsync(review => review.Id == id, cancellationToken);
        }

        public async Task<Review?> GetReviewWithImagesByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Reviews
                .Include(review => review.Images)
                .FirstOrDefaultAsync(review => review.Id == id, cancellationToken);
        }

        public async Task<(IReadOnlyList<Review> Items, int TotalCount)> GetReviewsByProductIdAsync(
            Guid productId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _appDbContext.Reviews
                .Where(review => review.ProductId == productId)
                .AsQueryable();

            var totalCount = await query.CountAsync(cancellationToken);

            var reviews = await query
                .OrderByDescending(review => review.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(review => review.Images)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (reviews, totalCount);
        }

        public async Task<bool> UserHasReviewedOrderProductAsync(
            Guid userId,
            Guid orderId,
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Reviews
                .IgnoreQueryFilters()
                .AnyAsync(review =>
                    review.UserId == userId &&
                    review.OrderId == orderId &&
                    review.ProductId == productId,
                    cancellationToken);
        }

        public async Task AddReviewAsync(Review review, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Reviews.AddAsync(review, cancellationToken);
        }

        public async Task UpdateReviewAsync(Review review, CancellationToken cancellationToken = default)
        {
            _appDbContext.Reviews.Update(review);
            await Task.CompletedTask;
        }

        public async Task AddReviewImageAsync(ReviewImage reviewImage, CancellationToken cancellationToken = default)
        {
            await _appDbContext.ReviewImages.AddAsync(reviewImage, cancellationToken);
        }

        public async Task<ReviewImage?> GetReviewImageByIdAsync(Guid imageId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.ReviewImages
                .FirstOrDefaultAsync(reviewImage => reviewImage.Id == imageId, cancellationToken);
        }

        public async Task RemoveReviewImageAsync(ReviewImage reviewImage, CancellationToken cancellationToken = default)
        {
            _appDbContext.ReviewImages.Remove(reviewImage);
            await Task.CompletedTask;
        }
    }
}
