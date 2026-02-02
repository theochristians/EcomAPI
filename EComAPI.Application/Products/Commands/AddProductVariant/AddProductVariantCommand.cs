namespace EComAPI.Application.Products.Commands.AddProductVariant
{
    public class AddProductVariantCommand
    {
        public Guid ProductId { get; }
        public string Sku { get; }
        public int Stock { get; }
        public decimal PriceAdjustment { get; }
        public string? Size { get; }
        public string? Color { get; }

        public AddProductVariantCommand(
            Guid productId,
            string sku,
            int stock,
            decimal priceAdjustment,
            string? size,
            string? color)
        {
            ProductId = productId;
            Sku = sku;
            Stock = stock;
            PriceAdjustment = priceAdjustment;
            Size = size;
            Color = color;
        }
    }
}