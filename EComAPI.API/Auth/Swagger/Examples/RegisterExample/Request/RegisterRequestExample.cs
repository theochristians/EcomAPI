using EComAPI.API.Auth.Dtos.Request;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.RegisterExample.Request
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