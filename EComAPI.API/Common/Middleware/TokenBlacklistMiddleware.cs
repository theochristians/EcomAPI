using EComAPI.API.Common;
using EComAPI.API.Common.Security;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Security;
using System.Net;

namespace EComAPI.API.Common.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ITokenBlacklistRepository tokenBlacklistRepository)
        {
            var endpoint = context.GetEndpoint();
            var allowAnonymous =
                endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null;

            if (allowAnonymous)
            {
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].ToString();
            var token = AuthorizationHeaderHelper.ExtractBearerToken(authHeader);

            if (!string.IsNullOrWhiteSpace(token))
            {
                var tokenHash = TokenHelper.HashToken(token);
                var isBlacklisted = await tokenBlacklistRepository.IsBlacklistedAsync(tokenHash);

                if (isBlacklisted)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "application/json";

                    var response = ApiResponse<object>.Fail("Token has been revoked");
                    await context.Response.WriteAsJsonAsync(response);

                    return;
                }
            }

            await _next(context);
        }
    }
}
