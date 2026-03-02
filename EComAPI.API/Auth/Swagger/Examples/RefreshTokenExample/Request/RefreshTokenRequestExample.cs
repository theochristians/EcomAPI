using EComAPI.API.Auth.Dtos.Request;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.RefreshTokenExample.Request
{
    public class RefreshTokenRequestExample
        : IExamplesProvider<RefreshTokenRequest>
    {
        public RefreshTokenRequest GetExamples()
        {
            return new RefreshTokenRequest(
                "d4f8e3b2-example-refresh-token-value"
            );
        }
    }
}
