namespace HRManagement.Api.Domain.Models.Response.Shared
{
    public class ApiResponse
    {
        public string Title { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public string StatusMessage { get; set; } = string.Empty;

        public bool IsError { get; set; }

        public dynamic Content { get; set; } = string.Empty;
    }
}
