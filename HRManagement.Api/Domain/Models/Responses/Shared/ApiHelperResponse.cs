using System.Net;

namespace HRManagement.Api.Domain.Models.Responses.Shared
{
    public class ApiHelperResponse
    {
        public static ApiResponse Success()
        {
            return new ApiResponse()
            {
                Title = "Success",
                StatusCode = (int)HttpStatusCode.OK,
                IsError = false
            };
        }

        public static ApiResponse<T> Success<T>(T data)
        {
            return new ApiResponse<T>()
            {
                Title = "Success",
                StatusCode = (int)HttpStatusCode.OK,
                IsError = false,
                Content = data
            };
        }

        public static ApiResponse<T> Success<T>(string message, T data)
        {
            return new ApiResponse<T>()
            {
                Title = "Success",
                StatusCode = (int)HttpStatusCode.OK,
                IsError = false,
                Content = data,
                StatusMessage = message
            };
        }

        public static ApiResponse SuccessWithError(string message)
        {
            return new ApiResponse()
            {
                Title = "Success",
                StatusCode = (int)HttpStatusCode.OK,
                IsError = true,
                StatusMessage = message
            };
        }

        public static ApiResponse Failed(string errorMessage)
        {
            return new ApiResponse()
            {
                Title = "Error",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                StatusMessage = errorMessage,
                IsError = true
            };
        }

        public static ApiResponse Failed(string errorMessage, dynamic dataerror)
        {
            return new ApiResponse()
            {
                Title = "Error",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                StatusMessage = errorMessage,
                IsError = true,
                Content = dataerror
            };
        }

        public static ApiResponse Failed(string errorMessage, List<string> listErrors)
        {
            return new ApiResponse()
            {
                Title = "Error",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                StatusMessage = errorMessage,
                IsError = true,
                Content = listErrors
            };
        }
    }
}
