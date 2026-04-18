namespace EComAPI.Application.Transaction.Commands.PaymentCommands.SubmitPaymentProof
{
    public record SubmitPaymentProofCommand(
        Guid OrderId,
        string ProofImageUrl,
        string? PaymentMethod = null);
}
