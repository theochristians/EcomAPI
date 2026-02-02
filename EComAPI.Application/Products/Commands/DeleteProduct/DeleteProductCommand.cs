namespace EComAPI.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommand
    {
        public Guid Id { get; }

        public DeleteProductCommand(Guid id)
        {
            Id = id;
        }
    }
}