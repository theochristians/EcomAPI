namespace EComAPI.Application.Transaction.DTOs
{
    public record ReturnImageDto(
        Guid Id,
        string ImageUrl,
        string? Description,
        DateTime CreatedAt);
}
