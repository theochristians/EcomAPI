using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.RegisterExample.Response
{
    public class RegisterBadRequestExample : 
        IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
        {
            return ApiResponse<object>.Fail("Email already registered");
        }
    }
}