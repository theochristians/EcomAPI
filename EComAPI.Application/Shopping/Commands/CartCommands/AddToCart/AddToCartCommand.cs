namespace EComAPI.Application.Shopping.Commands.CartCommands.AddToCart
{
    public record AddToCartCommand(
        Guid ProductVariantId, 
        int Quantity
        );
}
