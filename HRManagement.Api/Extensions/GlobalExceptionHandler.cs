using HRManagement.Domain.Models.Response.Shared;

using Microsoft.AspNetCore.Diagnostics;

namespace HRManagement.Api.Extensions;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IWebHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Catat Log Error
        logger.LogError(exception, "Terjadi kesalahan sistem yang tidak tertangani.");

        var statusCode = StatusCodes.Status500InternalServerError;
        var message = "Terjadi kesalahan internal pada server. Silakan hubungi administrator.";
        object? devErrorDetails = null;

        // 2. 🔥 JIKA DEVELOPMENT: Bongkar isi pesan error ke dalam response
        if (env.IsDevelopment())
        {
            // Tampilkan pesan error utama
            message = exception.Message;

            // Masukkan detail stack trace ke dalam properti Content
            devErrorDetails = new
            {
                ExceptionType = exception.GetType().Name,
                InnerException = exception.InnerException?.Message,
                StackTrace = exception.StackTrace
            };
        }

        // 3. Buat ApiResponse
        var response = new ApiResponse<object>
        {
            Title = "Internal Server Error",
            StatusCode = statusCode,
            StatusMessage = message,
            IsError = true,
            Content = devErrorDetails // Akan null di Production, tapi berisi stack trace di Development
        };

        // 4. Kirim response ke client
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
