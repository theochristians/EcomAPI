using EComAPI.API.Auth.Dtos.Request;
using EComAPI.Domain.Auth.Enums;
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
                "Password123",
                "081234567890",
                new DateTime(2000, 1, 1),
                Gender.Male
            );
        }
    }
}