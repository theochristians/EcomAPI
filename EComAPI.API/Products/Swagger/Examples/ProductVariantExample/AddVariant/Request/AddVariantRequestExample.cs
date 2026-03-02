using EComAPI.API.Products.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductVariantExample.AddVariant.Request
{
    public class AddProductVariantRequestExample : IExamplesProvider<AddProductVariantRequest>
    {
        public AddProductVariantRequest GetExamples()
        {
            return new AddProductVariantRequest(
                Sku: "NIKE-AIRMAX90-42-BLK",
                Stock: 50,
                PriceAdjustment: 200000,
                Size: "42",
                Color: "Black"
            );
        }
    }
}