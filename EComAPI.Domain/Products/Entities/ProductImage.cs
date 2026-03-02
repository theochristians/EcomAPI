using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Products.Entities
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public Product? Product { get; private set; }
        public string ImageUrl { get; private set; }
        public bool IsPrimary { get; private set; }
        public int DisplayOrder { get; private set; }

        private ProductImage() { }

        public ProductImage(
            Guid productId,
            string imageUrl,
            Guid createdBy,
            bool isPrimary = false,
            int displayOrder = 0)
        {
            Guard.AgainstEmptyGuid(productId, "ProductId is required");

            var imageUrlProductImageValue = Guard.AgainstNullOrWhiteSpace(imageUrl, "Image URL is required");
            Guard.AgainstMaxLength(imageUrlProductImageValue, 500, "Image URL cannot exceed 500 characters");

            Guard.AgainstNegative(displayOrder, "Display order cannot be negative");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required"); 

            ProductId = productId;
            ImageUrl = imageUrlProductImageValue;
            IsPrimary = isPrimary;
            DisplayOrder = displayOrder;
            SetCreated(createdBy);
        }

        public void Update(
            string imageUrl,
            bool isPrimary,
            int displayOrder,
            Guid updatedBy)
        {
            var imageUrlProductImageValue = Guard.AgainstNullOrWhiteSpace(imageUrl, "Image URL is required");
            Guard.AgainstMaxLength(imageUrlProductImageValue, 500, "Image URL cannot exceed 500 characters");
           
            Guard.AgainstNegative(displayOrder, "Display order cannot be negative");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            ImageUrl = imageUrlProductImageValue;
            IsPrimary = isPrimary;
            DisplayOrder = displayOrder;

            SetUpdated(updatedBy);
        }

        public void SetAsPrimary(Guid updatedBy)
        {
            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            IsPrimary = true;
            SetUpdated(updatedBy);
        }

        public void UnsetPrimary()
        {
            IsPrimary = false;
        }
    }
}