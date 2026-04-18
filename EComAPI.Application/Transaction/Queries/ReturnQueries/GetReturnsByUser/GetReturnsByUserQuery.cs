namespace EComAPI.Application.Transaction.Queries.ReturnQueries.GetReturnsByUser
{
    public record GetReturnsByUserQuery(
        int Page = 1,
        int PageSize = 10);
}
