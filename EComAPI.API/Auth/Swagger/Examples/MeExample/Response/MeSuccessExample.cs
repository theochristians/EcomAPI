using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.MeExample.Response
{
    public class MeSuccessExample : IExamplesProvider<ApiResponse<UserProfileResponse>>
    {
        public ApiResponse<UserProfileResponse> GetExamples()
        {
            return ApiResponse<UserProfileResponse>.Ok(
                new UserProfileResponse(
                    Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    "John Doe",
                    "john@mail.com",
                    true
                ),
                "Current user profile"
            );
        }
    }
}
