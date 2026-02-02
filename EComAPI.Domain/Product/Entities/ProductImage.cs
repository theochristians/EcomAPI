using EComAPI.Domain.Common.Base;

namespace EComAPI.Domain.Product.Entities
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public string ImageUrl { get; private set; }
        public bool IsPrimary { get; private set; }
        public int DisplayOrder { get; private set; }

        protected ProductImage() { }

        public ProductImage(
            Guid productId,
            string imageUrl,
            bool isPrimary = false,
            int displayOrder = 0)
        {
            ProductId = productId;
            ImageUrl = imageUrl;
            IsPrimary = isPrimary;
            DisplayOrder = displayOrder;
        }

        public void SetPrimary()
        {
            IsPrimary = true;
        }

        public void UnsetPrimary()
        {
            IsPrimary = false;
        }

    }
}
