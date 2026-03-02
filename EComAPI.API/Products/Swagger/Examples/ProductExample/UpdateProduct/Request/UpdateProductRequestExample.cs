using EComAPI.API.Products.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductExample.UpdateProduct.Request
{
    public class UpdateProductRequestExample : IExamplesProvider<UpdateProductRequest>
    {
        public UpdateProductRequest GetExamples()
            => new(
                Name: "Nike Air Max 90 Premium",
                Slug: "nike-air-max-90-premium",
                BasePrice: 1650000,
                CategoryId: Guid.Parse("c1f7f3a1-8c34-4b12-9b5f-1a2b3c4d5e6f"),
                Description: "Versi premium dengan material yang lebih nyaman."
            );
    }
}
