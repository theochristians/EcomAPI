using Microsoft.AspNetCore.Mvc;

namespace EComAPI.API.Common
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult SuccessResponse<T>(T data, string message = "Success")
            => Ok(ApiResponse<T>.Ok(data, message));

        protected IActionResult BadRequestResponse(string message = "Fail")
            => BadRequest(ApiResponse<object>.Fail(message));

        protected IActionResult UnauthorizedResponse(string message = "Unauthorized")
            => Unauthorized(ApiResponse<object>.Fail(message));

        protected IActionResult NotFoundResponse(string message = "Not Found")
            => NotFound(ApiResponse<object>.Fail(message));
    }
}