using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductVariantExample.UpdateVariant.Response
{
    public class UpdateProductVariantSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(
                new { variantId = Guid.Parse("d4e5f6a7-b8c9-0123-def0-456789012345") },
                "Variant updated successfully"
            );
    }
}
