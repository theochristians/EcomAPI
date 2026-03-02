using EComAPI.API.Products.Dtos.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductVariantExample.UpdateVariant.Request
{
    public class UpdateProductVariantRequestExample : IExamplesProvider<UpdateProductVariantRequest>
    {
        public UpdateProductVariantRequest GetExamples()
            => new(
                Sku: "NIKE-AIRMAX90-42-BLK-REV1",
                Stock: 45,
                PriceAdjustment: 150000,
                Size: "42",
                Color: "Black"
            );
    }
}
