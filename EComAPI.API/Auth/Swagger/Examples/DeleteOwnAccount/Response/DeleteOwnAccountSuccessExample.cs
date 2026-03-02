using EComAPI.API.Common;
using Swashbuckle.AspNetCore.Filters;

namespace EComAPI.API.Auth.Swagger.Examples.DeleteOwnAccount.Response
{
    public class DeleteOwnAccountSuccessExample : IExamplesProvider<ApiResponse<object>>
    {
        public ApiResponse<object> GetExamples()
        {
            return ApiResponse<object>.Ok(
                new { userId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6") },
                "Account deleted permanently. You can register again with the same email if needed."
            );
        }
    }
}
