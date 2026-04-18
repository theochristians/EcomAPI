using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using EComAPI.Application.Common.Interfaces.Identity;

namespace EComAPI.Infrastructure.Common.Identity
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user == null || !user.Identity?.IsAuthenticated == true)
                    throw new UnauthorizedAccessException("User is not authenticated");

                // Try "sub" claim first (JWT standard)
                var subClaim = user.FindFirstValue("sub");
                if (!string.IsNullOrEmpty(subClaim) && Guid.TryParse(subClaim, out var subGuid))
                    return subGuid;

                // Fallback to NameIdentifier
                var nameIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(nameIdClaim) && Guid.TryParse(nameIdClaim, out var nameGuid))
                    return nameGuid;

                throw new UnauthorizedAccessException("User ID not found in claims");
            }
        }

        public string? Email =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ??
            _httpContextAccessor.HttpContext?.User?.FindFirstValue("email");

        public string? FullName =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ??
            _httpContextAccessor.HttpContext?.User?.FindFirstValue("name");

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool HasPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                return false;

            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            return user.Claims.Any(claim =>
                claim.Type == "permission" &&
                string.Equals(claim.Value, permission, StringComparison.Ordinal));
        }
    }
}
