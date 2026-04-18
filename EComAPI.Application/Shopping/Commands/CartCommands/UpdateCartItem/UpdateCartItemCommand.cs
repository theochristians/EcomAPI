namespace EComAPI.Application.Shopping.Commands.CartCommands.UpdateCartItem
{
    public record UpdateCartItemCommand(
        Guid CartItemId, 
        int Quantity
        );
}
