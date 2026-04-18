namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record SubmitPaymentProofRequest(
        string ProofImageUrl,
        string? PaymentMethod = null);
}
