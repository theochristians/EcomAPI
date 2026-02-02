using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EComAPI.API.Auth.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet("secure")]
        [Authorize]
        public IActionResult Secure()
        {
            return Ok("JWT OK");
        }
    }
}
