using EComAPI.Domain.Common.Base;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Common.Guards;

namespace EComAPI.Domain.Products.Entities
{
    public class Product : BaseEntity
    {
        public Guid CategoryId { get; private set; }
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public string? Description { get; private set; }
        public decimal BasePrice { get; private set; }
        public int ViewCount { get; private set; }
        public int TotalStock { get; private set; }  
        public bool IsActive { get; private set; }

        private readonly List<ProductVariant> _variants = new();
        public IReadOnlyCollection<ProductVariant> Variants => _variants;

        private readonly List<ProductImage> _images = new();
        public IReadOnlyCollection<ProductImage> Images => _images;

        private Product() { }

        public Product(
            Guid categoryId,
            string name,
            string slug,
            decimal basePrice,
            Guid createdBy,
            string? description = null)
        {
            Guard.AgainstEmptyGuid(categoryId, "CategoryId is required");

            var nameProductValue = Guard.AgainstNullOrWhiteSpace(name, "Product name is required");
            Guard.AgainstMaxLength(nameProductValue, 255, "Product name cannot exceed 255 characters");

            var slugProductValue = Guard.AgainstNullOrWhiteSpace(slug, "Product slug is required");
            Guard.AgainstMaxLength(slugProductValue, 300, "Product slug cannot exceed 300 characters");

            var descriptionProductValue = Guard.AgainstMaxLengthIfProvided(description, 1000, "Description cannot exceed 1000 characters");

            Guard.AgainstNegative(basePrice, "Base price cannot be negative");

            Guard.AgainstEmptyGuid(createdBy, "CreatedBy is required");

            CategoryId = categoryId;
            Name = nameProductValue;
            Slug = slugProductValue.ToLowerInvariant();
            BasePrice = basePrice;
            Description = descriptionProductValue;
            IsActive = true;
            ViewCount = 0;
            TotalStock = 0;  

            SetCreated(createdBy);
        }

        public void AddVariant(ProductVariant productVariant)
        {
            Guard.AgainstNull(productVariant, "ProductVariant is required");
            Guard.AgainstEmptyGuid(productVariant.Id, "ProductVariant ID is required");

            if (_variants.Any(v => v.Sku == productVariant.Sku && v.DeletedAt == null))
                throw new DomainException($"Variant with SKU '{productVariant.Sku}' already exists");

            _variants.Add(productVariant);
            RecalculateTotalStock(); 
        }

        public void RemoveVariant(Guid variantId)
        {
            var variant = _variants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null)
                throw new DomainException("Variant not found");

            _variants.Remove(variant);
            RecalculateTotalStock();  
        }

        public void AddImage(ProductImage productImage)
        {
            Guard.AgainstNull(productImage, "ProductImage is required");
            Guard.AgainstEmptyGuid(productImage.Id, "ProductImage ID is required");

            if (productImage.IsPrimary)
            {
                foreach (var img in _images)
                    img.UnsetPrimary();
            }

            _images.Add(productImage);
        }

        public void RemoveImage(Guid imageId)
        {
            var image = _images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
                throw new DomainException("Image not found");

            _images.Remove(image);
        }

        public void IncreaseView()
        {
            ViewCount++;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            string slug,
            decimal basePrice,
            Guid updatedBy,
            string? description = null,
            Guid? categoryId = null)
        {
            var nameProductValue = Guard.AgainstNullOrWhiteSpace(name, "Product name is required");
            Guard.AgainstMaxLength(nameProductValue, 255, "Product name cannot exceed 255 characters");

            var slugProductValue = Guard.AgainstNullOrWhiteSpace(slug, "Product slug is required");
            Guard.AgainstMaxLength(slugProductValue, 300, "Product slug cannot exceed 300 characters");

            Guard.AgainstNegative(basePrice, "Base price cannot be negative");

            var descriptionProductValue = Guard.AgainstMaxLengthIfProvided(description, 1000, "Description cannot exceed 1000 characters");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            Name = nameProductValue;
            Slug = slugProductValue.ToLowerInvariant();
            BasePrice = basePrice;
            Description = descriptionProductValue;

            if (categoryId.HasValue)
            {
                Guard.AgainstEmptyGuid(categoryId.Value, "CategoryId is required");
                CategoryId = categoryId.Value;
            }
            SetUpdated(updatedBy);
        }

        public void RecalculateTotalStock()
        {
            TotalStock = _variants
                .Where(v => v.DeletedAt == null)  
                .Sum(v => v.Stock);
        }

        public override void Delete(Guid deletedBy)
        {
            if (IsDeleted)
                throw new DomainException("Product is already deleted");

            Guard.AgainstEmptyGuid(deletedBy, "DeletedBy is required");

            foreach (var variant in _variants.Where(v => !v.IsDeleted))
            {
                variant.Delete(deletedBy);
            }

            foreach (var image in _images.Where(i => !i.IsDeleted))
            {
                image.Delete(deletedBy);
            }

            IsActive = false;

            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }

        public override void Restore()
        {
            if (!IsDeleted)
                throw new DomainException("Product is not deleted");

            foreach (var variant in _variants.Where(v => v.IsDeleted))
            {
                variant.Restore();
            }

            foreach (var image in _images.Where(i => i.IsDeleted))
            {
                image.Restore();
            }

            IsActive = true;

            DeletedAt = null;
            DeletedBy = null;
        }

        public void Deactivate(Guid updatedBy)
        {
            if (!IsActive)
                throw new DomainException("Product is already inactive");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            IsActive = false;
            SetUpdated(updatedBy);
        }

        public void Activate(Guid updatedBy)
        {
            if (IsActive)
                throw new DomainException("Product is already active");

            if (IsDeleted)
                throw new DomainException("Cannot activate deleted product");

            Guard.AgainstEmptyGuid(updatedBy, "UpdatedBy is required");

            IsActive = true;
            SetUpdated(updatedBy);
        }
    }
}
