namespace EComAPI.Application.Transaction.DTOs
{
    public record ReviewImageDto(
        Guid Id,
        string ImageUrl,
        int DisplayOrder);
}
