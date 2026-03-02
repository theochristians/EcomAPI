using Microsoft.AspNetCore.Authorization;

namespace EComAPI.API.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var permissionClaims = context.User.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToHashSet();

            if (permissionClaims.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning(
                    "Permission denied. Required: '{Permission}'. User: {UserId}",
                    requirement.Permission,
                    context.User.Identity?.Name ?? "anonymous");
            }

            return Task.CompletedTask;
        }
    }
}