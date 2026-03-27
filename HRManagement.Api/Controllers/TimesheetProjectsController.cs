using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using HRManagement.Api.Application.Commands.Timesheet;
using HRManagement.Api.Application.Queries.Timesheet;
using HRManagement.Api.Application.TimesheetDtos.Commands.Dto;
using HRManagement.Api.Application.TimesheetDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/timesheet/projects")]
public class TimesheetProjectsController : ValidateController<TimesheetProjectsController>
{
    private readonly ILogger<TimesheetProjectsController> _logger;
    private readonly IMediator _mediator;

    public TimesheetProjectsController(
        IMediator mediator,
        ILogger<TimesheetProjectsController> logger,
        IEnumerable<IValidator> validators) : base(validators, logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Fetches the list of all active projects for timesheet dropdowns
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProjectDto>>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<ProjectDto>>>> GetProjects()
    {
        var methodName = nameof(GetProjects);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetProjectListQuery();
        return await this.ValidateAndExecute<ApiResponse<List<ProjectDto>>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetProjectListQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Opens a new project that interns can log against
    [HttpPost]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> CreateProject(
        [FromBody] CreateProjectRequestDto requestDto)
    {
        var methodName = nameof(CreateProject);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new CreateProjectCommand(requestDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((CreateProjectCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Edit project info or mark it as Finished
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> UpdateProject(
        int id,
        [FromBody] UpdateProjectRequestDto requestDto)
    {
        var methodName = nameof(UpdateProject);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new UpdateProjectCommand(id, requestDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((UpdateProjectCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
}
