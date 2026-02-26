using MediatR;
using Microsoft.AspNetCore.Mvc;
using CSharpFunctionalExtensions;
using FluentValidation;
using HRManagement.Api.Application.Queries;
using HRManagement.Api.Application.Auth.DTOs;
using HRManagement.Api.Domain.Models.Responses.Shared;

namespace HRManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ValidateController<AuthController>
{
    private readonly ILogger<AuthController> _logger;
    private readonly IMediator _mediator;
    
    public AuthController(
        ILogger<AuthController> logger,
        IMediator mediator,
        IEnumerable<IValidator> validators) : base(validators, logger)
    {
        _logger = logger;
        _mediator = mediator;
    }
    
    [HttpPost("login")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), 200)]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        string methodName = nameof(Login);
        _logger.LogInformation("Start {Service} for {Email}.", methodName, request.Email);
        
        var query = new LoginQuery(request.Email, request.Password, request.RememberMe);
        var response = await ValidateAndExecute<ApiResponse<TokenResponseDto>>(query, async (q) => 
        {
            var apiResponse = await _mediator.Send((LoginQuery)q);
            return Result.Success(apiResponse);

        }).ConfigureAwait(false);

        _logger.LogInformation("End {Service}.", methodName);
        return response;
        
    }
}
