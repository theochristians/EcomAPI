using EComAPI.API.Auth.DTOs.Responses;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.LoginExample.Response
{
    public class LoginSuccessExample
        : IExamplesProvider<ApiResponse<AuthResponse>>
    {
        public ApiResponse<AuthResponse> GetExamples()
        {
            return ApiResponse<AuthResponse>.Ok(
                new AuthResponse(
                    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.example.jwt.token"
                ),
                "Login successful"
            );
        }
    }
}