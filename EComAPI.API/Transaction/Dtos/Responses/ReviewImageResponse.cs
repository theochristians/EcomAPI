namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record ReviewImageResponse(
        Guid Id,
        string ImageUrl,
        int DisplayOrder);
}
