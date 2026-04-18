namespace EComAPI.Application.Shopping.DTOs
{
    public record CartDto(
        Guid Id,
        Guid UserId,
        IReadOnlyList<CartItemDto> Items,
        decimal TotalPrice,
        int TotalItems
    );
}
