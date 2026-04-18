namespace EComAPI.Application.Shopping.DTOs
{
    public record CartItemDto(
        Guid Id,
        Guid ProductVariantId,
        string ProductName,
        string VariantSku,
        string? VariantSize,
        string? VariantColor,
        decimal UnitPrice,
        int Quantity,
        decimal Subtotal
    );
}
