namespace EComAPI.Application.Transaction.DTOs
{
    public record OrderStatusLogDto(
        Guid Id,
        Guid OrderId,
        string Status,
        string? Note,
        DateTime ChangedAt,
        Guid? CreatedBy);
}
