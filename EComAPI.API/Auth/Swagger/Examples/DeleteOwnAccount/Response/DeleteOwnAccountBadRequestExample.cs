using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.DeleteOwnAccount.Response
{
    public class DeleteOwnAccountBadRequestExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
        {
            return ApiResponse<object>.Fail("Failed to delete account");
        }
    }
}
