namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record CreateReviewImageRequest(
        string ImageUrl,
        int DisplayOrder = 0);
}
