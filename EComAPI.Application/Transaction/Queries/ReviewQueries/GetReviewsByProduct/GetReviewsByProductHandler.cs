using EComAPI.Application.Common.Result;
using EComAPI.Application.Transaction.DTOs;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.Application.Transaction.Queries.ReviewQueries.GetReviewsByProduct
{
    public class GetReviewsByProductHandler
    {
        private readonly IReviewRepository _reviewRepository;

        public GetReviewsByProductHandler(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<Result<(IReadOnlyList<ReviewDto> Items, int TotalCount)>> Handle(
            GetReviewsByProductQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var (reviews, totalCount) = await _reviewRepository.GetReviewsByProductIdAsync(
                    query.ProductId, query.Page, query.PageSize, cancellationToken);

                var reviewDtos = reviews
                    .Select(reviewEntity => new ReviewDto(
                        reviewEntity.Id,
                        reviewEntity.UserId,
                        reviewEntity.OrderId,
                        reviewEntity.ProductId,
                        reviewEntity.Rating,
                        reviewEntity.Comment,
                        reviewEntity.CreatedAt,
                        reviewEntity.Images.Select(reviewImage => new ReviewImageDto(
                            reviewImage.Id,
                            reviewImage.ImageUrl,
                            reviewImage.DisplayOrder)).ToList()))
                    .ToList();

                return Result<(IReadOnlyList<ReviewDto>, int)>.Success((reviewDtos, totalCount));
            }
            catch (DomainException domainException)
            {
                return Result<(IReadOnlyList<ReviewDto>, int)>.Failure(domainException.Message);
            }
            catch (Exception)
            {
                return Result<(IReadOnlyList<ReviewDto>, int)>.Failure("An error occurred while retrieving reviews");
            }
        }
    }
}
