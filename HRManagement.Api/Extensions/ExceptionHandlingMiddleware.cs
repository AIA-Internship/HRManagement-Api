using FluentValidation;

using HRManagement.Api.Domain.Models.Constants;
using HRManagement.Api.Domain.Models.Response.Shared;

using System.Text.Json;

namespace HRManagement.Api.Extensions
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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

            var errorResponse = new ApiResponse
            {
                Title = GetTitle(exception),
                StatusCode = GetStatusCode(exception),
                StatusMessage = exception.Message,
                IsError = true,
                Content = exception.Data
            };

            if (exception.Message.Contains("No authenticationScheme was specified"))
            {
                errorResponse.StatusCode = StatusCodes.Status401Unauthorized;
                errorResponse.StatusMessage = ExceptionConstants.NotAuthorizedExcepction;
            }


            _logger.LogError(exception.Message);
            var result = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(result);
        }


        public static int GetStatusCode(Exception exception) =>
            exception switch
            {
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
