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
[Route("api/timesheet")]
public class TimesheetEntriesController : ValidateController<TimesheetEntriesController>
{
    private readonly ILogger<TimesheetEntriesController> _logger;
    private readonly IMediator _mediator;

    public TimesheetEntriesController(
        IMediator mediator,
        ILogger<TimesheetEntriesController> logger,
        IEnumerable<IValidator> validators) : base(validators, logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Get timesheet view for a specific day
    [HttpGet("daily")]
    [ProducesResponseType(typeof(ApiResponse<DailyTimesheetResponseDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<DailyTimesheetResponseDto>>> GetDailyTimesheet(
        [FromQuery] string date,
        [FromQuery] int? targetEmployeeId)
    {
        var methodName = nameof(GetDailyTimesheet);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetDailyTimesheetQuery(date, targetEmployeeId);
        return await this.ValidateAndExecute<ApiResponse<DailyTimesheetResponseDto>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetDailyTimesheetQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Get timesheet view for a specific week period
    [HttpGet("weekly")]
    [ProducesResponseType(typeof(ApiResponse<WeeklyTimesheetResponseDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<WeeklyTimesheetResponseDto>>> GetWeeklyTimesheet(
        [FromQuery] string weekStartDate,
        [FromQuery] int? targetEmployeeId)
    {
        var methodName = nameof(GetWeeklyTimesheet);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetWeeklyTimesheetQuery(weekStartDate, targetEmployeeId);
        return await this.ValidateAndExecute<ApiResponse<WeeklyTimesheetResponseDto>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetWeeklyTimesheetQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Get timesheet view for an entire month grid
    [HttpGet("monthly")]
    [ProducesResponseType(typeof(ApiResponse<MonthlyTimesheetResponseDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<MonthlyTimesheetResponseDto>>> GetMonthlyTimesheet(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int? targetEmployeeId)
    {
        var methodName = nameof(GetMonthlyTimesheet);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetMonthlyTimesheetQuery(year, month, targetEmployeeId);
        return await this.ValidateAndExecute<ApiResponse<MonthlyTimesheetResponseDto>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetMonthlyTimesheetQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Save or update all timesheet entries for a single day
    [HttpPost("entry")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> SaveDailyTimesheet(
        [FromBody] SaveDailyTimesheetRequestDto requestDto)
    {
        var methodName = nameof(SaveDailyTimesheet);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new SaveDailyTimesheetCommand(requestDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((SaveDailyTimesheetCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
}
