namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record CreateReturnImageRequest(
        string ImageUrl,
        string? Description = null);
}
