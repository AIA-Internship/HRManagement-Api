namespace HRManagement.Api.Domain.Models.Response.Shared
{
    public class ServiceResult
    {
        public string? Message { get; set; }

        public bool IsError { get; set; }

        public dynamic? Content { get; set; }
    }
}