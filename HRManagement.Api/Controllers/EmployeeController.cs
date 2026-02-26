using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSharpFunctionalExtensions;
using FluentValidation;

using HRManagement.Api.Application.Commands;
using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Application.Queries;
using HRManagement.Api.Domain.Models.Responses.Shared;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ValidateController<EmployeeController>
{
    private readonly ILogger<EmployeeController> _logger;
    private readonly IMediator _mediator;
    
    public EmployeeController(
        IMediator mediator, 
        ILogger<EmployeeController> logger, 
        IEnumerable<IValidator> validators) : base(validators, logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    
    [HttpPut("me")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<EmployeeProfileResponseDto>>> UpdateEmployee([FromBody] UpdateEmployeeRequestDto commandDto)
    {
        string methodName = nameof(UpdateEmployee);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var command = new UpdateEmployeeCommand(commandDto);
        return await this.ValidateAndExecute<ApiResponse<EmployeeProfileResponseDto>>(command, async (c) => 
        {
            var result = await _mediator.Send((UpdateEmployeeCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
    
    [HttpPut("employment-info/{id}")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> UpdateEmploymentInformation(int id, [FromBody] UpdateEmploymentInfoRequestDto commandDto)
    {
        string methodName = nameof(UpdateEmploymentInformation);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var command = new UpdateEmployeeInfoCommand(id, commandDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) => 
        {
            var result = await _mediator.Send((UpdateEmployeeInfoCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
    
    [Authorize(Roles = "Supervisor")]
    [HttpGet("list")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<EmployeeListItemDto>>>> GetAllEmployees()
    {
        string methodName = nameof(GetAllEmployees);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var query = new GetEmployeeListQuery();
        return await this.ValidateAndExecute<ApiResponse<List<EmployeeListItemDto>>>(query, async (q) => 
        {
            var result = await _mediator.Send((GetEmployeeListQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
    
    [HttpGet("me")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<EmployeeProfileResponseDto>>> GetMyProfile()
    {
        string methodName = nameof(GetMyProfile);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var query = new GetMyProfileQuery();
        return await this.ValidateAndExecute<ApiResponse<EmployeeProfileResponseDto>>(query, async (q) => 
        {
            var result = await _mediator.Send((GetMyProfileQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    [HttpGet("requests")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<EmployeeRequestResponseDto>>>> GetEmployeeRequests([FromQuery] int? status)
    {
        string methodName = nameof(GetEmployeeRequests);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var query = new GetUpdateRequestQuery(status);
        return await this.ValidateAndExecute<ApiResponse<List<EmployeeRequestResponseDto>>>(query, async (q) => 
        {
            var result = await _mediator.Send((GetUpdateRequestQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<EmployeeProfileResponseDto>>> GetEmployeeProfileById(int id)
    {
        string methodName = nameof(GetEmployeeProfileById);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var query = new GetEmployeeProfileByIdQuery(id);
        return await this.ValidateAndExecute<ApiResponse<EmployeeProfileResponseDto>>(query, async (q) => 
        {
            var result = await _mediator.Send((GetEmployeeProfileByIdQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
    
    [HttpPost("review-update")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> ReviewUpdate([FromBody] ReviewUpdateRequestDto decision)
    {
        string methodName = nameof(ReviewUpdate);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var command = new ReviewUpdateCommand(decision);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) => 
        {
            var result = await _mediator.Send((ReviewUpdateCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
    
    [HttpPost("create")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> CreateEmployee([FromBody] CreateEmployeeRequestDto requestDto)
    {
        string methodName = nameof(CreateEmployee);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var command = new CreateEmployeeCommand(requestDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) => 
        {
            var result = await _mediator.Send((CreateEmployeeCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
}
