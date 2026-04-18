namespace EComAPI.Application.Transaction.Commands.ReviewCommands.CreateReview
{
    public record CreateReviewCommand(
        Guid OrderId,
        Guid ProductId,
        int Rating,
        string? Comment = null,
        List<CreateReviewImageDto>? Images = null);

    public record CreateReviewImageDto(
        string ImageUrl,
        int DisplayOrder = 0);
}
