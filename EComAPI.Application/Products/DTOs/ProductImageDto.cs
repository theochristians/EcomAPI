namespace EComAPI.Application.Products.DTOs
{
    public class ProductImageDto
    {
        public Guid Id { get; }
        public string ImageUrl { get; }
        public bool IsPrimary { get; }
        public int DisplayOrder { get; }

        public ProductImageDto(
            Guid id,
            string imageUrl,
            bool isPrimary,
            int displayOrder)
        {
            Id = id;
            ImageUrl = imageUrl;
            IsPrimary = isPrimary;
            DisplayOrder = displayOrder;
        }
    }
}