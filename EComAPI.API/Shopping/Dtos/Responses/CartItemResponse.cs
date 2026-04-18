namespace EComAPI.API.Shopping.Dtos.Responses
{
    public record CartItemResponse(
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
