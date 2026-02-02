namespace EComAPI.Application.Products.Commands.RestoreProduct
{
    public class RestoreProductCommand
    {
        public Guid Id { get; }

        public RestoreProductCommand(Guid id)
        {
            Id = id;
        }
    }
}