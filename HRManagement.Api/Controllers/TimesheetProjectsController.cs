using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using HRManagement.Application.Commands.Timesheet;
using HRManagement.Application.Queries.Timesheet;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Commands.Dto;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Queries.Dto;
using HRManagement.Domain.Models.Response.Shared;

namespace HRManagement.Controllers;

// [Authorize] - Temporarily disabled for debugging
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

    // ── GET /api/timesheet/projects ───────────────────────────────────────────
    // Returns all active projects (used by dropdown in timesheet entry & project list page)

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

    // ── POST /api/timesheet/projects ──────────────────────────────────────────
    // Creates a single project (supervisor only)

    [HttpPost]
    // [Authorize(Roles = "Supervisor")]
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

    // ── PUT /api/timesheet/projects/{id} ──────────────────────────────────────
    // Updates a single project (supervisor only)

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

    // ── PUT /api/timesheet/projects/bulk ──────────────────────────────────────
    // Replaces the full project list from the Edit Project page (supervisor only).
    // Creates new, updates existing, and soft-deletes removed projects in one transaction.

    [HttpPut("bulk")]
    // [Authorize] // Temporarily relaxed from [Authorize(Roles = "Supervisor")] for debugging
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> BulkUpsertProjects(
        [FromBody] BulkUpsertProjectsRequestDto requestDto)
    {
        var methodName = nameof(BulkUpsertProjects);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new BulkUpsertProjectsCommand(requestDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((BulkUpsertProjectsCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // ── DELETE /api/timesheet/projects/{id} ───────────────────────────────────
    // Soft-deletes a single project (supervisor only)

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteProject(int id)
    {
        var methodName = nameof(DeleteProject);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new DeleteProjectCommand(id);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((DeleteProjectCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
}



