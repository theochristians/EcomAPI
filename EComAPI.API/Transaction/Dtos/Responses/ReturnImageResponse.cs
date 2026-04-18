namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record ReturnImageResponse(
        Guid Id,
        string ImageUrl,
        string? Description,
        DateTime CreatedAt);
}
