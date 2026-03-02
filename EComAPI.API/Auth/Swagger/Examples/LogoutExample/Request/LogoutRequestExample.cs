using EComAPI.API.Auth.Dtos.Request;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.LogoutExample.Request
{
    public class LogoutRequestExample
        : IExamplesProvider<LogoutRequest>
    {
        public LogoutRequest GetExamples()
        {
            return new LogoutRequest(
                RefreshToken: "d4f8e3b2-example-refresh-token-value"
            );
        }
    }
}
