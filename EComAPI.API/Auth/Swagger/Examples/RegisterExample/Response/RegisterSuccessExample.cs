using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.RegisterExample.Response
{
    public class RegisterSuccessExample
        : IExamplesProvider<ApiResponse<RegisterResponse>>
    {
        public ApiResponse<RegisterResponse> GetExamples()
        {
            return ApiResponse<RegisterResponse>.Ok(
                new RegisterResponse(
                    Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    "John Doe",
                    "john@mail.com",
                    false
                ),
                "User registered successfully"
            );
        }
    }
}