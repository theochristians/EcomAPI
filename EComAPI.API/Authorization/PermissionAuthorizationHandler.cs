using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using EComAPI.Application.Auth.Interfaces;

namespace EComAPI.API.Authorization
{
    public class PermissionAuthorizationHandler
        : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)
                              ?? context.User.FindFirst("sub");

            if (userIdClaim == null)
                return;

            var userId = Guid.Parse(userIdClaim.Value);

            if (await _permissionService.HasPermissionAsync(userId, requirement.Permission))
                context.Succeed(requirement);
        }
    }
}