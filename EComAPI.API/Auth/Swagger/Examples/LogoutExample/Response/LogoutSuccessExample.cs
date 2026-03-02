using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.LogoutExample.Response
{
    public class LogoutSuccessExample
        : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
        {
            return ApiResponse<object>.Ok(
                new { message = "Logged out successfully" },
                "Logout successful"
            );
        }
    }
}
