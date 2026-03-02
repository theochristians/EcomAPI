using EComAPI.API.Products.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductImageExample.AddImage.Request
{
    public class AddProductImageRequestExample : IExamplesProvider<AddProductImageRequest>
    {
        public AddProductImageRequest GetExamples()
        {
            return new AddProductImageRequest(
                ImageUrl: "https://example.com/images/nike-airmax90-1.jpg",
                IsPrimary: true,
                DisplayOrder: 1
            );
        }
    }
}