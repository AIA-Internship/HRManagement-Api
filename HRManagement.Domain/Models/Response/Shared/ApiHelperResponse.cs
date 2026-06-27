using System.Net;
using CSharpFunctionalExtensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HRManagement.Domain.Models.Response.Shared
{
    public class ApiHelperResponse
    {
        public static Result<ApiResponse> Success()
        {
            var resp = new ApiResponse()
            {
                Title = "Success",
                StatusCode = (int)HttpStatusCode.OK,
                IsError = false
            };
            return Result.Success(resp);
        }

        public static Result<ApiResponse<T>> Success<T>(T data)
        {
            var resp = new ApiResponse<T>()
            {
                Title = "Success",
                StatusCode = (int)HttpStatusCode.OK,
                IsError = false,
                Content = data
            };
            return Result.Success(resp);
        }

        public static Result<ApiResponse<T>> Success<T>(string message, T data)
        {
            var resp = new ApiResponse<T>()
            {
                Title = "Success",
                StatusCode = (int)HttpStatusCode.OK,
                IsError = false,
                Content = data,
                StatusMessage = message
            };
            return Result.Success(resp);
        }

        public static Result<ApiResponse> SuccessWithError(string message)
        {
            var resp = new ApiResponse()
            {
                Title = "Success",
                StatusCode = (int)HttpStatusCode.OK,
                IsError = true,
                StatusMessage = message
            };
            return Result.Success(resp);
        }

        public static Result<ApiResponse> Failed(string errorMessage)
        {
            var resp = new ApiResponse()
            {
                Title = "Error",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                StatusMessage = errorMessage,
                IsError = true
            };
            return Result.Failure<ApiResponse>(errorMessage);
        }

        /// <summary>
        /// Generic overload — use when the method return type is <see cref="ApiResponse{T}"/>.
        /// </summary>
        public static Result<ApiResponse<T>> Failed<T>(string errorMessage)
        {
            var resp = new ApiResponse<T>()
            {
                Title = "Error",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                StatusMessage = errorMessage,
                IsError = true
            };
            return Result.Failure<ApiResponse<T>>(errorMessage);
        }

        /// <summary>
        /// Generic overload that includes data payload when returning a failed result.
        /// </summary>
        public static Result<ApiResponse<T>> Failed<T>(string errorMessage, T data)
        {
            var resp = new ApiResponse<T>()
            {
                Title = "Error",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                StatusMessage = errorMessage,
                IsError = true,
                Content = data
            };
            return Result.Failure<ApiResponse<T>>(errorMessage);
        }

        public static Result<ApiResponse> Failed(string errorMessage, dynamic dataerror)
        {
            var resp = new ApiResponse()
            {
                Title = "Error",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                StatusMessage = errorMessage,
                IsError = true,
                Content = dataerror
            };
            return Result.Failure<ApiResponse>(errorMessage);
        }

        public static Result<ApiResponse> Failed(string errorMessage, List<string> listErrors)
        {
            var resp = new ApiResponse()
            {
                Title = "Error",
                StatusCode = (int)HttpStatusCode.InternalServerError,
                StatusMessage = errorMessage,
                IsError = true,
                Content = listErrors
            };
            return Result.Failure<ApiResponse>(errorMessage);
        }

        public static Result<ApiResponse> NotFound(string errorMessage)
        {
            var resp = new ApiResponse()
            {
                Title = "Error",
                StatusCode = (int)HttpStatusCode.NotFound,
                StatusMessage = errorMessage,
                IsError = true,
            };
            return Result.Failure<ApiResponse>(errorMessage);
        }
    }
}
