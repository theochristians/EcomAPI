using EComAPI.API.Auth.Dtos.Response;
using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.GetOwnProfile.Response
{
    public class GetOwnProfileSuccessExample : IExamplesProvider<ApiResponse<UserProfileResponse>>
    {
        public ApiResponse<UserProfileResponse> GetExamples()
        {
            return ApiResponse<UserProfileResponse>.Ok(
                new UserProfileResponse(
                    Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), 
                    "John Doe",                                     
                    "john@example.com",                             
                    null,                                           
                    true,                                           
                    true,
                    "Customer",                                       
                    null,                                            
                    new DateTime(1990, 1, 1),                     
                    "Male",                                       
                    DateTime.Parse("2024-02-01T15:30:00Z"),        
                    null,                                          
                    DateTime.Parse("2021-06-15T00:00:00Z")         
                ),
                "Profile retrieved successfully"
            );
        }
    }
}