using EComAPI.API.Auth.Dtos.Request;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.LoginExample.Request
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