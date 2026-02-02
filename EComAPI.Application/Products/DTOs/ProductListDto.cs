namespace EComAPI.Application.Products.DTOs
{
    public class ProductListDto
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Slug { get; }
        public decimal BasePrice { get; }
        public int ViewCount { get; }

        public ProductListDto(
            Guid id,
            string name,
            string slug,
            decimal basePrice,
            int viewCount)
        {
            Id = id;
            Name = name;
            Slug = slug;
            BasePrice = basePrice;
            ViewCount = viewCount;
        }
    }
}