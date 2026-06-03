using System.Text.Json.Serialization;

namespace HRManagement.Api.Domain.Models.Response.Shared;

public class ApiResponse
{
    public string Title { get; set; } = "Success";
    public int StatusCode { get; set; } = 200;
    public string StatusMessage { get; set; } = string.Empty;
    public bool IsError { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Content { get; set; }
}

public class ApiResponse<T> : ApiResponse
{
    [JsonPropertyOrder(99)]
    public new T? Content { get; set; }

    public static ApiResponse<T> Success(string message, T? content = default)
    {
        return new ApiResponse<T>
        {
            StatusMessage = message,
            Content = content,
            IsError = false,
            StatusCode = 200
        };
    }

    public static ApiResponse<T> Failed(string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Title = "Error",
            StatusMessage = message,
            IsError = true,
            StatusCode = statusCode
        };
    }
}


