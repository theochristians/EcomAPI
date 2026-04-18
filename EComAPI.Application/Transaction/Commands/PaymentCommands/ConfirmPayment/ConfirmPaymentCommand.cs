namespace EComAPI.Application.Transaction.Commands.PaymentCommands.ConfirmPayment
{
    public record ConfirmPaymentCommand(
        Guid OrderId,
        string? AdminNote = null);
}
