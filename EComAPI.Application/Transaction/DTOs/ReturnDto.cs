namespace EComAPI.Application.Transaction.DTOs
{
    public record ReturnDto(
        Guid Id,
        Guid OrderId,
        Guid UserId,
        string ReturnNumber,
        string Reason,
        string Status,
        DateTime RequestedAt,
        DateTime? ApprovedAt,
        Guid? ApprovedBy,
        decimal RefundAmount,
        string? BankName,
        string? BankAccountNumber,
        string? AccountHolderName,
        DateTime? RefundDate,
        DateTime CreatedAt,
        IReadOnlyList<ReturnItemDto> Items,
        IReadOnlyList<ReturnImageDto> Images);
}
