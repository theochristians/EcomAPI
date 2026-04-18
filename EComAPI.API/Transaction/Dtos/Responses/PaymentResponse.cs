namespace EComAPI.API.Transaction.Dtos.Responses
{
    public record PaymentResponse(
        Guid Id,
        string? PaymentMethod,
        decimal Amount,
        string Status,
        string? ProofImageUrl,
        string? AdminNote,
        DateTime? ConfirmedAt);
}
