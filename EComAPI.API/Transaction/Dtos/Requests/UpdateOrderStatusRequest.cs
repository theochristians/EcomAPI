namespace EComAPI.API.Transaction.Dtos.Requests
{
    public record UpdateOrderStatusRequest(
        string Status,
        string? Courier = null,
        string? TrackingNumber = null,
        string? AdminNote = null);
}
