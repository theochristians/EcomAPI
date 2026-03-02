using System.Net;
using System.Text.Json;
using EComAPI.Domain.Common.Exceptions;

namespace EComAPI.API.Common
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _nextRequestDelegate;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate NextRequestDelegate,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _nextRequestDelegate = NextRequestDelegate;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _nextRequestDelegate(httpContext);
            }
            catch (DomainException domainException)
            {
                _logger.LogWarning(domainException, "Domain exception");

                await WriteResponse(
                    httpContext,
                    HttpStatusCode.BadRequest,
                    domainException.Message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                await WriteResponse(
                    httpContext,
                    HttpStatusCode.InternalServerError,
                    "Internal server error"
                );
            }
        }

        private static async Task WriteResponse(
            HttpContext httpContext,
            HttpStatusCode httpStatusCode,
            string message)
        {
            httpContext.Response.StatusCode = (int)httpStatusCode;
            httpContext.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(message);

            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(response)
            );
        }
    }
}