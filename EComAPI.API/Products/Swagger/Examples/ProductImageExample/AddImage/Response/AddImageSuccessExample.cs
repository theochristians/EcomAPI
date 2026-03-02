using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.ProductImageExample.AddImage.Response
{
    public class AddProductImageSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
        {
            return ApiResponse<object>.Ok(
                new
                {
                    imageId = Guid.Parse("d4e5f6a7-b8c9-0123-def0-456789012345")
                },
                "Image added success"
            );
        }
    }
}