<<<<<<< HEAD
using System.Text.Json.Serialization;
=======
﻿using System.Text.Json.Serialization;
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a

namespace HRManagement.Api.Domain.Models.Response.Shared;

public class ApiResponse
{
    public string Title { get; set; } = "Success";
    public int StatusCode { get; set; } = 200;
    public string StatusMessage { get; set; } = string.Empty;
    public bool IsError { get; set; }
<<<<<<< HEAD
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
=======
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
    public object? Content { get; set; }
}

public class ApiResponse<T> : ApiResponse
{
    [JsonPropertyOrder(99)]
    public new T? Content { get; set; }
}
