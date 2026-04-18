namespace EComAPI.Application.Transaction.DTOs
{
    public record ReviewDto(
        Guid Id,
        Guid UserId,
        Guid OrderId,
        Guid ProductId,
        int Rating,
        string? Comment,
        DateTime CreatedAt,
        IReadOnlyList<ReviewImageDto> Images);
}
