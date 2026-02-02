using EComAPI.Domain.Common.Base;

namespace EComAPI.Domain.Product.Entities
{
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; private set; }
        public string Sku { get; private set; }
        public string? Size { get; private set; }
        public string? Color { get; private set; }
        public decimal PriceAdjustment { get; private set; }
        public int Stock { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        protected ProductVariant() { }

        public ProductVariant(
            Guid productId,
            string sku,
            int stock,
            decimal priceAdjustment = 0,
            string? size = null,
            string? color = null)
        {
            ProductId = productId;
            Sku = sku;
            Stock = stock;
            PriceAdjustment = priceAdjustment;
            Size = size;
            Color = color;
            IsActive = true;
        }

        public void DecreaseStock(int quantity)
        {
            if (Stock < quantity)
                throw new InvalidOperationException("Insufficient stock");

            Stock -= quantity;
        }

        public void IncreaseStock(int quantity)
        {
            Stock += quantity;
        }

        public void SoftDelete()
        {
            DeletedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

    }
}
