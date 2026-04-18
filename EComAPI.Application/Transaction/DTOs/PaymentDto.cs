namespace EComAPI.Application.Transaction.DTOs
{
    public record PaymentDto(
        Guid Id,
        string? PaymentMethod,
        decimal Amount,
        string Status,
        string? ProofImageUrl,
        string? AdminNote,
        DateTime? ConfirmedAt);
}
