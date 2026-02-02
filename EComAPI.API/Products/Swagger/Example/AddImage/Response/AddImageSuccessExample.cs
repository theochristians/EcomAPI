using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Products.Swagger.Examples.AddImage.Response
{
    public class AddImageSuccessExample
        : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
            => ApiResponse<object>.Ok(null, "Success image added");
    }
}