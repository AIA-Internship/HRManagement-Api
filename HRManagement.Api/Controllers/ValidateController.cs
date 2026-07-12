using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HRManagement.Domain.Models.Response.Shared;

namespace HRManagement.Api.Controllers;

public abstract class ValidateController<T> : ControllerBase
{
    protected readonly IMediator _mediator;
    protected readonly ILogger<T> _logger;
    protected readonly IEnumerable<IValidator>? _validators;

    protected ValidateController(IMediator mediator, ILogger<T> logger, IEnumerable<IValidator>? validators = null)
    {
        _mediator = mediator;
        _logger = logger;
        _validators = validators;
    }

    public async Task<ActionResult<ApiResponse>> ValidateAndExecute<TRequest, TResponse>(TRequest request, Func<TRequest, Task<Result<ApiResponse<TResponse>>>> handler)
    {
        // validation could be implemented here using _validators if provided
        var result = await handler(request).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            var val = result.Value;
            return new ApiResponse
            {
                Title = val.Title,
                StatusCode = val.StatusCode,
                IsError = val.IsError,
                StatusMessage = val.StatusMessage,
                Content = val.Content
            };
        }

        return new ApiResponse
        {
            Title = "Error",
            StatusCode = 500,
            IsError = true,
            StatusMessage = result.Error
        };
    }

    public async Task<ActionResult<ApiResponse>> ValidateAndExecute<TRequest>(TRequest request, Func<TRequest, Task<Result<ApiResponse>>> handler)
    {
        var result = await handler(request).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            var val = result.Value;
            return new ApiResponse
            {
                Title = val.Title,
                StatusCode = val.StatusCode,
                IsError = val.IsError,
                StatusMessage = val.StatusMessage,
                Content = val.Content
            };
        }

        return new ApiResponse
        {
            Title = "Error",
            StatusCode = 500,
            IsError = true,
            StatusMessage = result.Error
        };
    }

    // Support handlers that return a non-generic Result (no payload / ApiResponse wrapper)
    public async Task<ActionResult<ApiResponse>> ValidateAndExecute<TRequest>(TRequest request, Func<TRequest, Task<Result>> handler)
    {
        var result = await handler(request).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            return new ApiResponse
            {
                Title = "Success",
                StatusCode = 200,
                IsError = false
            };
        }

        return new ApiResponse
        {
            Title = "Error",
            StatusCode = 500,
            IsError = true,
            StatusMessage = result.Error
        };
    }
}
