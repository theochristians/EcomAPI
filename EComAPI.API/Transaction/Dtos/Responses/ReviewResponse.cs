namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record ReviewResponse(
        Guid Id,
        Guid UserId,
        Guid OrderId,
        Guid ProductId,
        int Rating,
        string? Comment,
        DateTime CreatedAt,
        List<ReviewImageResponse> Images);
}
