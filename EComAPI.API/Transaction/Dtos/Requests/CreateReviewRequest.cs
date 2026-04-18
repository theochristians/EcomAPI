namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record CreateReviewRequest(
        Guid OrderId,
        Guid ProductId,
        int Rating,
        string? Comment = null,
        List<CreateReviewImageRequest>? Images = null);
}
