namespace EComAPI.Application.Transaction.Commands.OrderCommands.UpdateOrderStatus
{
    public record UpdateOrderStatusCommand(
        Guid OrderId,
        string NewStatus,
        string? Courier = null,
        string? TrackingNumber = null,
        string? AdminNote = null);
}
