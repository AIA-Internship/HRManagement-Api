namespace HRManagement.Api.Domain.Models.Responses.Shared
{
    public class ApiException : Exception
    {
        public ApiException() { }
        
        public ApiException(string message) : base(message) { }
        
        public ApiException(string title, int statusCode, string message) : base(message)
        {
            Title = title;
            StatusCode = statusCode;
        }

        public string Title { get; set; }
        public int StatusCode { get; set; }
        public ServiceResult? Result { get; set; }
    }
}