using System.Text.Json.Serialization;

namespace HRManagement.Domain.Models.Response.Shared;

public class ApiResponse<T>
{
    public string Title { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string StatusMessage { get; set; } = string.Empty;
    public bool IsError { get; set; }

    // Ganti dynamic dengan tipe generic T
    public T? Content { get; set; }

    // Helper method untuk mempermudah (opsional tapi disarankan)
    public static ApiResponse<T> Success(T content, string message = "Success", int statusCode = 200) => new()
    {
        Title = "Success",
        StatusCode = statusCode,
        StatusMessage = message,
        IsError = false,
        Content = content
    };

    public static ApiResponse<T> Fail(string message, int statusCode = 400, string title = "Error") => new()
    {
        Title = title,
        StatusCode = statusCode,
        StatusMessage = message,
        IsError = true,
        Content = default
    };
}

public class ApiResponse : ApiResponse<object> { }
