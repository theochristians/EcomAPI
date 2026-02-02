using System.Security.Claims;

namespace EComAPI.API.Common.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal userClaimsPrincipal)
        {
            var userId = userClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? userClaimsPrincipal.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("UserId not found in token");

            return Guid.Parse(userId);
        }
    }
}
