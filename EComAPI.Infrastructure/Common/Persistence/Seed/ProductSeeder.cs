using EComAPI.Domain.Product.Entities;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Common.Persistence.Seed
{
    public static class ProductSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Products.AnyAsync())
                return;

            var product = new Product(
                categoryId: Guid.Empty, 
                name: "Sample Product",
                slug: "sample-product",
                basePrice: 100_000,
                description: "Sample seeded product"
            );

            context.Products.Add(product);
            await context.SaveChangesAsync();
        }
    }
}