using System.Text.Json.Serialization;

namespace HRManagement.Api.Domain.Models.Response.Shared;

public class ApiResponse
{
    public string Title { get; set; } = "Success";
    public int StatusCode { get; set; } = 200;
    public string StatusMessage { get; set; } = string.Empty;
    public bool IsError { get; set; }
    public object? Content { get; set; }
}

public class ApiResponse<T> : ApiResponse
{
    [JsonPropertyOrder(99)]
    public new T? Content { get; set; }
}
