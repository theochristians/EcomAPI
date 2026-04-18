namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record UpdateReviewRequest(
        int Rating,
        string? Comment = null);
}
