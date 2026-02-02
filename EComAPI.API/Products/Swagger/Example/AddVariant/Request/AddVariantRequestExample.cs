using EComAPI.API.Products.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.AddVariant.Request
{
    public class AddVariantRequestExample
        : IExamplesProvider<AddProductVariantRequest>
    {
        public AddProductVariantRequest GetExamples()
            => new(
                "NIKE-AIRMAX90-44-RED",
                25,
                250000,
                "44",
                "Red"
            );
    }
}