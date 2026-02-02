using System.Security.Claims;
using EComAPI.Application.Auth.Interfaces;

namespace EComAPI.API.Common.Identity
{
    public class CurrentUser : ICurrentUser
    {
        public Guid? UserId { get; }

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            var userId = httpContextAccessor.HttpContext?
                             .User?
                             .FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? httpContextAccessor.HttpContext?
                             .User?
                             .FindFirstValue("sub");

            if (Guid.TryParse(userId, out var id))
                UserId = id;
        }
    }
}