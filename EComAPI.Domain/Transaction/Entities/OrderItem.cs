using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Transaction.Entities
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; private set; }
        public Guid ProductVariantId { get; private set; }
        public string SnapshotProductName { get; private set; } = string.Empty;
        public string SnapshotVariantName { get; private set; } = string.Empty;
        public decimal SnapshotPrice { get; private set; }
        public int Quantity { get; private set; }

        private OrderItem() { }

        public OrderItem(
            Guid orderId,
            Guid productVariantId,
            string snapshotProductName,
            string snapshotVariantName,
            decimal snapshotPrice,
            int quantity,
            Guid createdBy)
        {
            Guard.AgainstEmptyGuid(orderId, "OrderId is required");
            Guard.AgainstEmptyGuid(productVariantId, "ProductVariantId is required");

            var productNameValue = Guard.AgainstNullOrWhiteSpace(snapshotProductName, "SnapshotProductName is required");
            Guard.AgainstMaxLength(productNameValue, 255, "SnapshotProductName cannot exceed 255 characters");

            var variantNameValue = Guard.AgainstNullOrWhiteSpace(snapshotVariantName, "SnapshotVariantName is required");
            Guard.AgainstMaxLength(variantNameValue, 100, "SnapshotVariantName cannot exceed 100 characters");

            Guard.AgainstNegative(snapshotPrice, "SnapshotPrice cannot be negative");
            if (quantity <= 0)
                throw new EComAPI.Domain.Common.Exceptions.DomainException("Quantity must be greater than zero");
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            OrderId = orderId;
            ProductVariantId = productVariantId;
            SnapshotProductName = productNameValue;
            SnapshotVariantName = variantNameValue;
            SnapshotPrice = snapshotPrice;
            Quantity = quantity;

            SetCreated(createdBy);
        }
    }
}
