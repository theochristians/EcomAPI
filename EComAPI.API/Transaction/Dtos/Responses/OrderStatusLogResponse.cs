namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record OrderStatusLogResponse(
        Guid Id,
        Guid OrderId,
        string Status,
        string? Note,
        DateTime ChangedAt,
        Guid? CreatedBy);
}
