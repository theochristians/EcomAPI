using EComAPI.API.Products.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.CreateProduct.Request
{
    public class CreateProductRequestExample
        : IExamplesProvider<CreateProductRequest>
    {
        public CreateProductRequest GetExamples()
            => new(
                Guid.Parse("c1f7f3a1-8c34-4b12-9b5f-1a2b3c4d5e6f"),
                "Nike Air Max 90",
                "nike-air-max-90",
                1500000,
                "Sepatu lari klasik dengan teknologi Air Max untuk kenyamanan maksimal."
            );
    }
}