namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record CreateReturnRequest(
        Guid OrderId,
        string Reason,
        List<CreateReturnItemRequest> Items,
        List<CreateReturnImageRequest>? Images = null,
        string? BankName = null,
        string? BankAccountNumber = null,
        string? AccountHolderName = null);
}
