using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.LoginExample.Response
{
    public class LoginSuccessExample
        : IExamplesProvider<ApiResponse<LoginResponse>>
    {
        public ApiResponse<LoginResponse> GetExamples()
        {
            return ApiResponse<LoginResponse>.Ok(
                new LoginResponse(
                    AccessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.example.access.token",
                    RefreshToken: "d4f8e3b2-example-refresh-token-value",
                    AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(120),
                    RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(7)
                ),
                "Login successful"
            );
        }
    }
}
