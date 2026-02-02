using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.LoginExample.Response
{
    public class LoginBadRequestExample
        : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
        {
            return ApiResponse<object>.Fail("Invalid credentials");
        }
    }

}