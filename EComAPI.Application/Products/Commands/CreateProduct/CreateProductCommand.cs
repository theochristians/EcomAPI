namespace EComAPI.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommand
    {
        public Guid CategoryId { get; }
        public string Name { get; }
        public string Slug { get; }
        public decimal BasePrice { get; }
        public string? Description { get; }

        public CreateProductCommand(
            Guid categoryId,
            string name,
            string slug,
            decimal basePrice,
            string? description)
        {
            CategoryId = categoryId;
            Name = name;
            Slug = slug;
            BasePrice = basePrice;
            Description = description;
        }
    }
}