using EComAPI.Domain.Product.Entities;

namespace EComAPI.Application.Products.DTOs
{
    public class ProductDetailDto
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Slug { get; }
        public decimal BasePrice { get; }
        public string? Description { get; }

        public IReadOnlyList<ProductVariantDto> Variants { get; }
        public IReadOnlyList<ProductImageDto> Images { get; }

        private ProductDetailDto(
            Guid id,
            string name,
            string slug,
            decimal basePrice,
            string? description,
            IReadOnlyList<ProductVariantDto> variants,
            IReadOnlyList<ProductImageDto> images)
        {
            Id = id;
            Name = name;
            Slug = slug;
            BasePrice = basePrice;
            Description = description;
            Variants = variants;
            Images = images;
        }

        public static ProductDetailDto From(Product product)
        {
            return new ProductDetailDto(
                product.Id,
                product.Name,
                product.Slug,
                product.BasePrice,
                product.Description,
                product.Variants.Select(v =>
                    new ProductVariantDto(
                        v.Id,
                        v.Sku,
                        v.Stock,
                        v.PriceAdjustment,
                        v.Size,
                        v.Color
                    )).ToList(),
                product.Images.Select(i =>
                    new ProductImageDto(
                        i.Id,
                        i.ImageUrl,
                        i.IsPrimary,
                        i.DisplayOrder
                    )).ToList()
            );
        }
    }
}