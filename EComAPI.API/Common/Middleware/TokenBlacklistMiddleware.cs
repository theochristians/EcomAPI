using EComAPI.API.Common;
using EComAPI.API.Common.Security;
using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Security;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Net;
using System.Security.Claims;

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
            ITokenBlacklistRepository tokenBlacklistRepository,
            AppDbContext appDbContext)
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

                var subClaim = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (Guid.TryParse(subClaim, out var userId))
                {
                    var latestUsedResetAt = await appDbContext.PasswordResets
                        .AsNoTracking()
                        .Where(passwordReset => passwordReset.UserId == userId && passwordReset.UsedAt != null)
                        .OrderByDescending(passwordReset => passwordReset.UsedAt)
                        .Select(passwordReset => passwordReset.UsedAt)
                        .FirstOrDefaultAsync(context.RequestAborted);

                    if (latestUsedResetAt.HasValue)
                    {
                        var issuedAt = ResolveIssuedAtUtc(context.User, token);

                        if (issuedAt <= latestUsedResetAt.Value)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = "application/json";

                            var response = ApiResponse<object>.Fail("Session expired due to password reset. Please login again");
                            await context.Response.WriteAsJsonAsync(response);

                            return;
                        }
                    }
                }
            }

            await _next(context);
        }

        private static DateTime ResolveIssuedAtUtc(ClaimsPrincipal principal, string rawToken)
        {
            var issuedAtClaim = principal.FindFirstValue(JwtRegisteredClaimNames.Iat);
            if (long.TryParse(issuedAtClaim, out var issuedAtEpochSeconds))
                return DateTimeOffset.FromUnixTimeSeconds(issuedAtEpochSeconds).UtcDateTime;

            var handler = new JsonWebTokenHandler();
            if (handler.CanReadToken(rawToken))
            {
                var jsonWebToken = handler.ReadJsonWebToken(rawToken);
                return jsonWebToken.ValidFrom;
            }

            return DateTime.MinValue;
        }
    }
}
