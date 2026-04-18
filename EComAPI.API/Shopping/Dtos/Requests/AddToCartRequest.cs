namespace EComAPI.API.Shopping.Dtos.Requests
{
    public record AddToCartRequest(
        Guid ProductVariantId,
        int Quantity = 1
    );
}
