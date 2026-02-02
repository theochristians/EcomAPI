using EComAPI.API.Auth.DTOs.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.RegisterExample.Request
{
    public class LoginRequestExample
        : IExamplesProvider<LoginRequest>
    {
        public LoginRequest GetExamples()
        {
            return new LoginRequest(
                "john@mail.com",
                "Password123"
            );
        }
    }
}