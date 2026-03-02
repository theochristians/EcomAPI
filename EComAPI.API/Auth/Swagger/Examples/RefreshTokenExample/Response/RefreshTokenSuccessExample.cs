using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.RefreshTokenExample.Response
{
    public class RefreshTokenSuccessExample
        : IExamplesProvider<ApiResponse<LoginResponse>>
    {
        public ApiResponse<LoginResponse> GetExamples()
        {
            return ApiResponse<LoginResponse>.Ok(
                new LoginResponse(
                    AccessToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.example.new.access.token",
                    RefreshToken: "a8c7d6e5-example-new-refresh-token",
                    AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(15),
                    RefreshTokenExpiresAt: DateTime.UtcNow.AddDays(7)
                ),
                "Token refreshed successfully"
            );
        }
    }
}
