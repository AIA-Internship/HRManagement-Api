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
        private readonly IWebHostEnvironment _env;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try 
            {
                await _next(httpContext);
                if (httpContext.Response.StatusCode is >= 400 and < 600 && !httpContext.Response.HasStarted)
                {
                    await HandleStatusCodeAsync(httpContext);
                }
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

            // In development include full exception details to help debugging
            if (_env != null && _env.IsDevelopment())
            {
                errorResponse.Content = new { correlationId, details = exception.ToString() };
            }

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
        
        private async Task HandleStatusCodeAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            var correlationId = GetOrCreateCorrelationId(context);
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            
            string title = context.Response.StatusCode switch
            {
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status403Forbidden => "Forbidden",
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status500InternalServerError => "Internal Server Error",
                _ => "Error"
            };
            
            string customMessage = context.Response.StatusCode switch
            {
                StatusCodes.Status401Unauthorized => ExceptionConstants.NotAuthorizedExcepction,
                StatusCodes.Status403Forbidden => ExceptionConstants.Forbidden,
                StatusCodes.Status400BadRequest => ExceptionConstants.BadRequest,
                StatusCodes.Status409Conflict => ExceptionConstants.Conflict,
                StatusCodes.Status500InternalServerError => ExceptionConstants.InternalServerError,
                _ => $"The server returned a {context.Response.StatusCode} status."
            };
            
            var errorResponse = new ApiResponse
            {
                Title = title, 
                StatusCode = context.Response.StatusCode, 
                StatusMessage = customMessage, 
                IsError = true,
                Content = new { correlationId }
            };

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
