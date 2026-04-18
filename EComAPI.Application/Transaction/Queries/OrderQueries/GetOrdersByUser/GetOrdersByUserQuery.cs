namespace EComAPI.Application.Transaction.Queries.OrderQueries.GetOrdersByUser
{
    public record GetOrdersByUserQuery(
        Guid UserId,
        int Page = 1,
        int PageSize = 10);
}
