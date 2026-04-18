namespace EComAPI.Application.Transaction.Commands.ReturnCommands.ApproveReturn
{
    public record ApproveReturnCommand(
        Guid ReturnId,
        decimal RefundAmount,
        List<ApproveReturnItemDto>? ItemConditions = null);

    public record ApproveReturnItemDto(
        Guid ReturnItemId,
        string Condition,
        string? AdminNote = null);
}
