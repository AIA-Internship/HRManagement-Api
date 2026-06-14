using CSharpFunctionalExtensions;

using FluentValidation;

using HRManagement.Application.Commands;
using HRManagement.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Application.Features.ESS.Employee.Queries;
using HRManagement.Application.Queries;
using HRManagement.Domain.Models.Response;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/employee")]
public class EmployeeController(ISender sender) : BaseApiController(sender)
{
    [HttpGet("list")]
    [HasPermission(Permissions.Users.View)]
    public async Task<ActionResult<ApiResponse<List<EmployeeListResponseDto>>>> GetAllEmployees()
    {
        string methodName = nameof(GetAllEmployees);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetEmployeeListQuery();
        return await this.ValidateAndExecute<ApiResponse<List<EmployeeListResponseDto>>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetEmployeeListQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    [HttpPut("employment-info/{displayId}")]
    [HasPermission(Permissions.Users.Edit)]
    public async Task<ActionResult<ApiResponse<string>>> UpdateEmploymentInformation(string displayId, [FromBody] UpdateEmploymentInfoRequestDto commandDto)
    {
        string methodName = nameof(UpdateEmploymentInformation);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var command = new UpdateEmployeeInfoCommand(displayId, commandDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) => 
        {
            var result = await _mediator.Send((UpdateEmployeeInfoCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
    
    [HttpPut("me")]
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
    
    
    
    [HttpGet("me")]
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

    [HttpGet("my-requests")]
    [HasPermission(Permissions.Employees.View)]
    public async Task<ActionResult<ApiResponse<List<EmployeeRequestResponseDto>>>> GetMyRequests([FromQuery] int? status)
    {
        string methodName = nameof(GetMyRequests);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var query = new GetMyUpdateRequestQuery(status);
        return await this.ValidateAndExecute<ApiResponse<List<EmployeeRequestResponseDto>>>(query, async (q) => 
        {
            var result = await _mediator.Send((GetMyUpdateRequestQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    [HttpGet("requests")]
    [HasPermission(Permissions.Users.View)]
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

    [HttpGet("{displayId}")]
    [HasPermission(Permissions.Employees.View)]
    public async Task<ActionResult<ApiResponse<EmployeeProfileResponseDto>>> GetEmployeeProfileByDisplayId(string displayId)
    {
        string methodName = nameof(GetEmployeeProfileByDisplayId);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var query = new GetEmployeeProfileByDisplayIdQuery(displayId);
        return await this.ValidateAndExecute<ApiResponse<EmployeeProfileResponseDto>>(query, async (q) => 
        {
            var result = await _mediator.Send((GetEmployeeProfileByDisplayIdQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
    
    [HttpGet("supervisors-lookup")]
    [HasPermission(Permissions.Employees.View)]
    public async Task<ActionResult<ApiResponse<List<SupervisorLookupResponseDto>>>> GetSupervisorLookup()
    {
        string methodName = nameof(GetSupervisorLookup);
        _logger.LogInformation("Start {Service}.", methodName);
        
        var query = new GetSupervisorLookupQuery();
        var result = await _mediator.Send(query);
        
        _logger.LogInformation("End {Service}.", methodName);
        return Ok(result);
    }
    
    [HttpPost("review-update")]
    [HasPermission(Permissions.Employees.Edit)]
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
    [HasPermission(Permissions.Employees.Create)]
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

    [HttpPost("{id}/attachments")]
    [Authorize]
    [Consumes( "multipart/form-data")]
    public async Task<ActionResult<ApiResponse>> UploadAttachments(int id, [FromForm] UploadAttachmentDto request)
    {
        string methodName = nameof(UploadAttachments);
        _logger.LogInformation("Start {Service}.", methodName);
    
        var command = new UploadAttachmentCommand(id, request.DocumentType, request.Files);
        return await this.ValidateAndExecute<ApiResponse>(command, async (c) =>
        {
            var result = await _mediator.Send((UploadAttachmentCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result); 
        });
    }
}
