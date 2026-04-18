namespace EComAPI.API.Shopping.Dtos.Responses
{
    public record CartResponse(
        Guid Id,
        Guid UserId,
        IReadOnlyList<CartItemResponse> Items,
        decimal TotalPrice,
        int TotalItems
    );
}
