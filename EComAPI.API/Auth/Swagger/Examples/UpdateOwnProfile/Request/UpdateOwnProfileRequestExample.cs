using EComAPI.API.Auth.Dtos.Request;
using EComAPI.Domain.Auth.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.UpdateOwnProfile.Request
{
    public class UpdateOwnProfileRequestExample : IExamplesProvider<UpdateOwnProfileRequest>
    {
        public UpdateOwnProfileRequest GetExamples()
        {
            return new UpdateOwnProfileRequest(
                "John Doe Updated",
                "john.updated@example.com",
                "+628123456789",
                "123 Updated Street, Jakarta",
                new DateTime(1990, 1, 1),
                Gender.Male
            );
        }
    }
}