namespace EComAPI.Application.Transaction.Queries.ReviewQueries.GetReviewsByProduct
{
    public record GetReviewsByProductQuery(
        Guid ProductId,
        int Page = 1,
        int PageSize = 10);
}
