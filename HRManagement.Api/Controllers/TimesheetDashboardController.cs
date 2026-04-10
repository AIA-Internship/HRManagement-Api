using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using HRManagement.Api.Application.Queries.Timesheet;
using HRManagement.Api.Application.TimesheetDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/timesheet")]
public class TimesheetDashboardController : ValidateController<TimesheetDashboardController>
{
    private readonly ILogger<TimesheetDashboardController> _logger;
    private readonly IMediator _mediator;

    public TimesheetDashboardController(
        IMediator mediator,
        ILogger<TimesheetDashboardController> logger,
        IEnumerable<IValidator> validators) : base(validators, logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Retrieves dashboard details for logged-in employee (Welcome, Countdowns, etc.)
    [HttpGet("dashboard")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponseDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<DashboardResponseDto>>> GetEmployeeDashboard()
    {
        var methodName = nameof(GetEmployeeDashboard);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetEmployeeDashboardQuery();
        return await this.ValidateAndExecute<ApiResponse<DashboardResponseDto>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetEmployeeDashboardQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Retrieves dashboard analytics and pending requests for supervisor
    [HttpGet("supervisor/dashboard")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(typeof(ApiResponse<SupervisorDashboardResponseDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<SupervisorDashboardResponseDto>>> GetSupervisorDashboard(
        [FromQuery] int? filterEmployeeId)
    {
        var methodName = nameof(GetSupervisorDashboard);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetSupervisorDashboardQuery { FilterEmployeeId = filterEmployeeId };
        return await this.ValidateAndExecute<ApiResponse<SupervisorDashboardResponseDto>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetSupervisorDashboardQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
}
