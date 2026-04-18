namespace EComAPI.Application.Transaction.Commands.ReturnCommands.CreateReturn
{
    public record CreateReturnCommand(
        Guid OrderId,
        string Reason,
        List<CreateReturnItemDto> Items,
        List<CreateReturnImageDto>? Images = null,
        string? BankName = null,
        string? BankAccountNumber = null,
        string? AccountHolderName = null);

    public record CreateReturnItemDto(
        Guid OrderItemId,
        int Quantity);

    public record CreateReturnImageDto(
        string ImageUrl,
        string? Description = null);
}
