using EComAPI.API.Products.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductImageExample.UpdateImage.Request
{
    public class UpdateProductImageRequestExample : IExamplesProvider<UpdateProductImageRequest>
    {
        public UpdateProductImageRequest GetExamples()
            => new(
                ImageUrl: "https://example.com/images/nike-airmax90-1-updated.jpg",
                IsPrimary: true,
                DisplayOrder: 1
            );
    }
}
