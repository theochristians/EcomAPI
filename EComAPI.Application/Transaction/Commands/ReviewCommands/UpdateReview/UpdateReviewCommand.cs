namespace EComAPI.Application.Transaction.Commands.ReviewCommands.UpdateReview
{
    public record UpdateReviewCommand(
        Guid ReviewId,
        int Rating,
        string? Comment = null);
}
