using EComAPI.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EComAPI.Infrastructure.Common.Identity
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                return Guid.TryParse(userId, out var id) ? id : null;
            }
        }
    }
}