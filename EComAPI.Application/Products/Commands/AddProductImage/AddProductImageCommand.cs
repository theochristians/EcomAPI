namespace EComAPI.Application.Products.Commands.AddProductImage
{
    public class AddProductImageCommand
    {
        public Guid ProductId { get; }
        public string ImageUrl { get; }
        public bool IsPrimary { get; }
        public int DisplayOrder { get; }

        public AddProductImageCommand(
            Guid productId,
            string imageUrl,
            bool isPrimary,
            int displayOrder)
        {
            ProductId = productId;
            ImageUrl = imageUrl;
            IsPrimary = isPrimary;
            DisplayOrder = displayOrder;
        }
    }
}