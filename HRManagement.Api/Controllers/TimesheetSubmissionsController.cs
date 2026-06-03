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
public class TimesheetSubmissionsController : ValidateController<TimesheetSubmissionsController>
{
    private readonly ILogger<TimesheetSubmissionsController> _logger;
    private readonly IMediator _mediator;

    public TimesheetSubmissionsController(
        IMediator mediator,
        ILogger<TimesheetSubmissionsController> logger,
        IEnumerable<IValidator> validators) : base(validators, logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Submits the currently active month's timesheet for supervisor review
    [HttpPost("submit")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> SubmitTimesheet(
        [FromBody] SubmitTimesheetRequestDto requestDto)
    {
        var methodName = nameof(SubmitTimesheet);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new SubmitTimesheetCommand(requestDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((SubmitTimesheetCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Checks the submission state for a specific month (Draft, Waiting, Approved, Revision)
    [HttpGet("submission/status")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<SubmissionStatusDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<SubmissionStatusDto>>> GetSubmissionStatus(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var methodName = nameof(GetSubmissionStatus);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetSubmissionStatusQuery(year, month);
        return await this.ValidateAndExecute<ApiResponse<SubmissionStatusDto>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetSubmissionStatusQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Read list of past monthly submissions by the employee
    [HttpGet("submission/history")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<List<SubmissionHistoryItemDto>>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<List<SubmissionHistoryItemDto>>>> GetSubmissionHistory()
    {
        var methodName = nameof(GetSubmissionHistory);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetSubmissionHistoryQuery();
        return await this.ValidateAndExecute<ApiResponse<List<SubmissionHistoryItemDto>>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetSubmissionHistoryQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Generates overview report for supervisors to see pending approvals
    [HttpGet("supervisor/report")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SupervisorApprovalReportDto>), 200)]

    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<SupervisorApprovalReportDto>>> GetApprovalReport()
    {
        var methodName = nameof(GetApprovalReport);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetApprovalReportQuery();
        return await this.ValidateAndExecute<ApiResponse<SupervisorApprovalReportDto>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetApprovalReportQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Get specific employee submission data with calendar breakdown for review
    [HttpGet("supervisor/review/{submissionId:int}")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(typeof(ApiResponse<SupervisorReviewResponseDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<SupervisorReviewResponseDto>>> GetTimesheetReview(int submissionId)
    {
        var methodName = nameof(GetTimesheetReview);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetTimesheetReviewQuery(submissionId);
        return await this.ValidateAndExecute<ApiResponse<SupervisorReviewResponseDto>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetTimesheetReviewQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Get employee submission data by ID/Month/Year (Allows review before official submission)
    [HttpGet("supervisor/review/anytime")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(typeof(ApiResponse<SupervisorReviewResponseDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<SupervisorReviewResponseDto>>> GetTimesheetReviewAnytime(
        [FromQuery] int employeeId, [FromQuery] int month, [FromQuery] int year)
    {
        var methodName = nameof(GetTimesheetReviewAnytime);
        _logger.LogInformation("Start {Service}.", methodName);

        var query = new GetTimesheetReviewQuery(employeeId, month, year);
        return await this.ValidateAndExecute<ApiResponse<SupervisorReviewResponseDto>>(query, async (q) =>
        {
            var result = await _mediator.Send((GetTimesheetReviewQuery)q);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }


    // Set submission status to approved ( Supervisor only )
    [HttpPost("supervisor/approve")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> ApproveTimesheet(
        [FromBody] ApproveTimesheetRequestDto requestDto)
    {
        var methodName = nameof(ApproveTimesheet);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new ApproveTimesheetCommand(requestDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((ApproveTimesheetCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }

    // Set submission status back with revision note ( Supervisor only )
    [HttpPost("supervisor/revision")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(typeof(ApiResponse<string>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<ApiResponse<string>>> GiveRevision(
        [FromBody] GiveRevisionRequestDto requestDto)
    {
        var methodName = nameof(GiveRevision);
        _logger.LogInformation("Start {Service}.", methodName);

        var command = new GiveRevisionCommand(requestDto);
        return await this.ValidateAndExecute<ApiResponse<string>>(command, async (c) =>
        {
            var result = await _mediator.Send((GiveRevisionCommand)c);
            _logger.LogInformation("End {Service}.", methodName);
            return Result.Success(result);
        });
    }
}
