using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.DeleteAddress.Response
{
    public class DeleteAddressSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
        {
            return ApiResponse<object>.Ok(
                new { addressId = Guid.Parse("9c2f7e31-7e8a-4683-92ef-1c9a5949310a") },
                "Address deleted successfully"
            );
        }
    }
}
