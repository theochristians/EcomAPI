using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Products.Entities
{
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public Product? Product { get; private set; }
        public string Sku { get; private set; }
        public string? Size { get; private set; }
        public string? Color { get; private set; }
        public decimal? PriceAdjustment { get; private set; }
        public int Stock { get; private set; }
        public bool IsActive { get; private set; }

        private ProductVariant() { }

        public ProductVariant(
            Guid productId,
            string sku,
            int stock,
            Guid createdBy,
            decimal priceAdjustment = 0,
            string? size = null,
            string? color = null)
        {
            Guard.AgainstEmptyGuid(productId, "ProductId is required");

            var skuProductValue = Guard.AgainstNullOrWhiteSpace(sku, "SKU is required");
            Guard.AgainstMaxLength(skuProductValue, 50, "SKU cannot exceed 50 characters");

            Guard.AgainstNegative(stock, "Stock cannot be negative");
            
            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            if (priceAdjustment < 0)
            throw new DomainException("Price adjustment cannot be negative");

            ProductId = productId;
            Sku = skuProductValue.ToUpperInvariant();
            Stock = stock;
            PriceAdjustment = priceAdjustment;
            Size = size?.Trim();
            Color = color?.Trim();
            IsActive = true;
            SetCreated(createdBy);
        }

        public void DecreaseStock(int quantity, Guid updatedBy)
        {
            if (quantity <= 0)
                throw new DomainException("Quantity must be greater than zero");

            if (Stock < quantity)
                throw new DomainException($"Insufficient stock. Available: {Stock}, Requested: {quantity}");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Stock -= quantity;
            SetUpdated(updatedBy);
        }

        public void IncreaseStock(int quantity, Guid updatedBy)
        {
            if (quantity <= 0)
                throw new DomainException("Quantity must be greater than zero");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Stock += quantity;
            SetUpdated(updatedBy);
        }

        public void SetStock(int stock, Guid updatedBy)
        {
            Guard.AgainstNegative(stock, "Stock cannot be negative");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Stock = stock;
            SetUpdated(updatedBy);
        }

        public void Update(
            string? size,
            string? color,
            decimal? priceAdjustment,
            Guid updatedBy,
            string sku,
            int stock)
        {
            var skuProductValue = Guard.AgainstNullOrWhiteSpace(sku, "SKU is required");
            Guard.AgainstMaxLength(skuProductValue, 50, "SKU cannot exceed 50 characters");

            Guard.AgainstNegative(stock, "Stock cannot be negative");

            Guard.AgainstNegative(priceAdjustment ?? 0, "Price adjustment cannot be negative");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Sku = skuProductValue.ToUpperInvariant();
            Stock = stock;
            Size = size?.Trim();
            Color = color?.Trim();
            PriceAdjustment = priceAdjustment;

            SetUpdated(updatedBy);
        }

        public override void Delete(Guid deletedBy)
        {
            base.Delete(deletedBy);
            IsActive = false;
        }

        public override void Restore()
        {
            base.Restore();
            IsActive = true;
        }
    }
}
