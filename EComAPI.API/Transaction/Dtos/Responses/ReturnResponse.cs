namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record ReturnResponse(
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
        List<ReturnItemResponse> Items,
        List<ReturnImageResponse> Images);
}
