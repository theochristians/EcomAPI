using EComAPI.Domain.Common.Base;

namespace EComAPI.Domain.Product.Entities
{
    public class Product : BaseEntity
    {
        public Guid CategoryId { get; private set; }
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public string? Description { get; private set; }
        public decimal BasePrice { get; private set; }
        public int ViewCount { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? DeletedAt { get; private set; }

        private readonly List<ProductVariant> _variants = new();
        public IReadOnlyCollection<ProductVariant> Variants => _variants;

        private readonly List<ProductImage> _images = new();
        public IReadOnlyCollection<ProductImage> Images => _images;

        protected Product() { }

        public Product(
            Guid categoryId,
            string name,
            string slug,
            decimal basePrice,
            string? description = null)
        {
            CategoryId = categoryId;
            Name = name;
            Slug = slug;
            BasePrice = basePrice;
            Description = description;
            IsActive = true;
            ViewCount = 0;
        }

        public void AddVariant(ProductVariant variant)
        {
            if (variant.ProductId != Id)
                throw new InvalidOperationException("Variant does not belong to this product");

            _variants.Add(variant);
        }

        public void AddImage(ProductImage image)
        {
            if (image.ProductId != Id)
                throw new InvalidOperationException("Image does not belong to this product");

            if (image.IsPrimary)
            {
                foreach (var img in _images)
                    img.UnsetPrimary();
            }

            _images.Add(image);
        }

        public void IncreaseView()
        {
            ViewCount++;
        }
        public void Deactivate()
        {
            IsActive = false;
        }

        public void SoftDelete(Guid? userId)
        {
            DeletedAt = DateTime.UtcNow;
            SetUpdated(userId);
        }

        public void Restore()
        {
            DeletedAt = null;
            SetUpdated(null);
        }
    }

}
