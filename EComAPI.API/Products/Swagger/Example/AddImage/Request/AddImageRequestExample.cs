using EComAPI.API.Products.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.AddImage.Request
{
    public class AddImageRequestExample
        : IExamplesProvider<AddProductImageRequest>
    {
        public AddProductImageRequest GetExamples()
            => new(
                "https://cdn.example.com/images/nike-airmax90-back.jpg",
                false,
                3
            );
    }
}