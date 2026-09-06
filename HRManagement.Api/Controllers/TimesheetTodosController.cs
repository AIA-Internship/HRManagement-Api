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

[Authorize]
[ApiController]
[Route("api/timesheet/todos")]
public class TimesheetTodosController : ValidateController<TimesheetTodosController>
{
    private readonly ILogger<TimesheetTodosController> _logger;
    private readonly IMediator _mediator;

    public TimesheetTodosController(
        IMediator mediator,
        ILogger<TimesheetTodosController> logger,
        IEnumerable<IValidator> validators) : base(validators, logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Fetches all active to-do tasks for the logged in dashboard view
    [HttpGet]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<List<TodoTaskResponseDto>>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<TodoTaskResponseDto>>>> GetTodoTasks()
    {
        var methodName = nameof(GetTodoTasks);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetTodoTasksQuery();
        return await this.ValidateAndExecute<ApiResponse<List<TodoTaskResponseDto>>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetTodoTasksQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Record a new personal to-do task (0 = Low, 1 = Medium, 2 = High priority)
    [HttpPost]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> CreateTodoTask(
        [FromBody] CreateTodoTaskRequestDto requestDto)
    {
        var methodName = nameof(CreateTodoTask);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new CreateTodoTaskCommand(requestDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((CreateTodoTaskCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Modify details of a specific to-do task
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTodoTask(
        int id,
        [FromBody] UpdateTodoTaskRequestDto requestDto)
    {
        var methodName = nameof(UpdateTodoTask);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new UpdateTodoTaskCommand(id, requestDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((UpdateTodoTaskCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Fast toggle for switching a task between checked/unchecked states
    [HttpPatch("{id:int}/toggle")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> ToggleTodoTask(int id)
    {
        var methodName = nameof(ToggleTodoTask);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new ToggleTodoTaskCommand(id);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((ToggleTodoTaskCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Permanently remove task from dashboard
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTodoTask(int id)
    {
        var methodName = nameof(DeleteTodoTask);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new DeleteTodoTaskCommand(id);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((DeleteTodoTaskCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
}



