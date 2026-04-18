using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using EComAPI.API.Common;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.API.DependencyInjection;

public static class RateLimitingDependencyInjection
{
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimitingOptions = configuration
            .GetSection(ApiRateLimitingOptions.SectionName)
            .Get<ApiRateLimitingOptions>()
            ?? throw new InvalidOperationException(
                $"Missing configuration section '{ApiRateLimitingOptions.SectionName}'.");

        Validate(rateLimitingOptions);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var response = ApiResponse<object>.Fail(rateLimitingOptions.RejectionMessage);
                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
            };

            options.AddPolicy("auth-register", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => CreateFixedWindowOptions(rateLimitingOptions.AuthRegister)));

            options.AddPolicy("auth-login", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => CreateFixedWindowOptions(rateLimitingOptions.AuthLogin)));

            options.AddPolicy("email-verify-send", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetUserIdOrIp(httpContext),
                    factory: _ => CreateFixedWindowOptions(rateLimitingOptions.EmailVerifySend)));

            options.AddPolicy("email-verify-check", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetUserIdOrIp(httpContext),
                    factory: _ => CreateFixedWindowOptions(rateLimitingOptions.EmailVerifyCheck)));

            options.AddPolicy("upload-sas", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetUserIdOrIp(httpContext),
                    factory: _ => CreateFixedWindowOptions(rateLimitingOptions.UploadSas)));

            options.AddPolicy("upload-confirm", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetUserIdOrIp(httpContext),
                    factory: _ => CreateFixedWindowOptions(rateLimitingOptions.UploadConfirm)));
        });

        return services;
    }

    private static FixedWindowRateLimiterOptions CreateFixedWindowOptions(
        FixedWindowPolicyOptions policyOptions)
    {
        return new FixedWindowRateLimiterOptions
        {
            PermitLimit = policyOptions.PermitLimit,
            Window = TimeSpan.FromMinutes(policyOptions.WindowMinutes),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            AutoReplenishment = true
        };
    }

    private static string GetClientIp(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string GetUserIdOrIp(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst("sub")?.Value ??
                     httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            return $"user:{userId}";
        }

        return $"ip:{GetClientIp(httpContext)}";
    }

    private static void Validate(ApiRateLimitingOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.RejectionMessage))
        {
            throw new InvalidOperationException(
                "RateLimiting:RejectionMessage cannot be empty.");
        }

        ValidatePolicy("AuthRegister", options.AuthRegister);
        ValidatePolicy("AuthLogin", options.AuthLogin);
        ValidatePolicy("EmailVerifySend", options.EmailVerifySend);
        ValidatePolicy("EmailVerifyCheck", options.EmailVerifyCheck);
        ValidatePolicy("UploadSas", options.UploadSas);
        ValidatePolicy("UploadConfirm", options.UploadConfirm);
    }

    private static void ValidatePolicy(
        string policyName,
        FixedWindowPolicyOptions policy)
    {
        if (policy.PermitLimit <= 0)
        {
            throw new InvalidOperationException(
                $"RateLimiting:{policyName}:PermitLimit must be greater than 0.");
        }

        if (policy.WindowMinutes <= 0)
        {
            throw new InvalidOperationException(
                $"RateLimiting:{policyName}:WindowMinutes must be greater than 0.");
        }
    }
}