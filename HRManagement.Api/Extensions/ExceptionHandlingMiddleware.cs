using System.ComponentModel.DataAnnotations;
using System.Text.Json;

using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;

namespace HRManagement.Api.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;
            var correlationId = GetOrCreateCorrelationId(context);
            response.Headers["X-Correlation-ID"] = correlationId;

            var errorResponse = new ApiResponse
            {
                Title = GetTitle(exception),
                StatusCode = GetStatusCode(exception),
                StatusMessage = exception.Message,
                IsError = true,
                Content = new { correlationId }
            };

            if (exception.Message.Contains("No authenticationScheme was specified"))
            {
                errorResponse.StatusCode = StatusCodes.Status401Unauthorized;
                errorResponse.StatusMessage = ExceptionConstants.NotAuthorizedExcepction;
            }

            _logger.LogError(exception, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);
            response.StatusCode = errorResponse.StatusCode;

            var result = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(result);
        }

        private static string GetOrCreateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var existing) && !string.IsNullOrWhiteSpace(existing))
            {
                return existing!;
            }

            return context.TraceIdentifier;
        }


        public static int GetStatusCode(Exception exception) =>
            exception switch
            {
                ApiException apiException => apiException.StatusCode,
                ApplicationException => StatusCodes.Status400BadRequest,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError

            };

        public static string GetTitle(Exception exception) =>
            exception switch
            {
                KeyNotFoundException => "Not Found",
                _ => "Error"
            };
    }
}
