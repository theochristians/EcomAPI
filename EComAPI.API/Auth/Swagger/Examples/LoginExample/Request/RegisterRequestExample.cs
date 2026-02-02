using EComAPI.API.Auth.DTOs.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.LoginExample.Request
{
    public class RegisterRequestExample
        : IExamplesProvider<RegisterRequest>
    {
        public RegisterRequest GetExamples()
        {
            return new RegisterRequest(
                "John Doe",
                "john@mail.com",
                "Password123"
            );
        }
    }
}