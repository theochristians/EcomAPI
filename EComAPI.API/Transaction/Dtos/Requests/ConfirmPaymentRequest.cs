namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record ConfirmPaymentRequest(
        string? AdminNote = null);
}
