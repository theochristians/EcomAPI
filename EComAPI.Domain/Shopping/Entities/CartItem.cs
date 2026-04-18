using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Shopping.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; private set; }
        public Guid ProductVariantId { get; private set; }
        public int Quantity { get; private set; }

        public Cart? Cart { get; private set; }

        private CartItem() { }

        public CartItem(
            Guid cartId,
            Guid productVariantId,
            int quantity,
            Guid createdBy)
        {
            Guard.AgainstEmptyGuid(cartId, "CartId is required");
            Guard.AgainstEmptyGuid(productVariantId, "ProductVariantId is required");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            if (quantity < 1)
                throw new DomainException("Quantity must be at least 1");

            CartId = cartId;
            ProductVariantId = productVariantId;
            Quantity = quantity;
            SetCreated(createdBy);
        }

        public void UpdateQuantity(int quantity, Guid updatedBy)
        {
            if (quantity < 1)
                throw new DomainException("Quantity must be at least 1");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Quantity = quantity;
            SetUpdated(updatedBy);
        }

        public void IncreaseQuantity(int amount)
        {
            if (amount < 1)
                throw new DomainException("Amount must be at least 1");

            Quantity += amount;
        }
    }
}
