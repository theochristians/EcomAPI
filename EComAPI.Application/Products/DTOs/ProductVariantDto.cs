namespace EComAPI.Application.Products.DTOs
{
    public class ProductVariantDto
    {
        public Guid Id { get; }
        public string Sku { get; }
        public int Stock { get; }
        public decimal PriceAdjustment { get; }
        public string? Size { get; }
        public string? Color { get; }

        public ProductVariantDto(
            Guid id,
            string sku,
            int stock,
            decimal priceAdjustment,
            string? size,
            string? color)
        {
            Id = id;
            Sku = sku;
            Stock = stock;
            PriceAdjustment = priceAdjustment;
            Size = size;
            Color = color;
        }
    }
}