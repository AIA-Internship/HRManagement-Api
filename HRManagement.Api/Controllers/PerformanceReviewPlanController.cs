using HRManagement.Application.Auth.Permissions;
using HRManagement.Application.Features.Performance_Review.Commands;
using HRManagement.Application.Features.PerformanceReview.Plans.Queries;
using HRManagement.Application.Features.PerformanceReview.Queries;
using HRManagement.Application.Features.PerformanceReviewPlan.Queries;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/plan")]
[Route("api/plans")]
[Route("api/performance-review/plans")]
public class PerformanceReviewPlanController(ISender sender) : BaseApiController(sender)
{

    [HttpGet("intervals/{intervalId:int}/employees/{employeeId:int}/reviewers/{assessmentType}")]
    public async Task<IActionResult> GetReviewersForEmployee(
    [FromRoute] int intervalId,
    [FromRoute] int employeeId,
    [FromRoute] string assessmentType,
    CancellationToken ct)
    {
        var query = new GetReviewersByEmployeeAndIntervalQuery(intervalId, employeeId, assessmentType);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("{planId}")]
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetPlanByIdAsync(int planId, CancellationToken ct)
    {
        var query = new GetPerformanceReviewPlanByIdQuery(planId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("all")]
    //[HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetAllPlansAsync(CancellationToken ct)
    {
        var query = new GetPerformanceReviewPlansQuery();
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("me/ongoing")]
    public async Task<IActionResult> GetEmployeeOngoingPerformanceReviewPlanAsync(
    CancellationToken ct)
    {
        var query = new GetEmployeeOngoingPerformanceReviewPlanQuery(CurrentEmployeeId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> GetPlanLookupsAsync(CancellationToken ct)
    {
        var query = new GetPerformanceReviewPlansLookupQuery();
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("{planId:int}/intervals")]
    public async Task<IActionResult> GetIntervalsByPlanIdAsync([FromRoute] int planId, CancellationToken ct)
    {
        var query = new GetIntervalsByPlanIdQuery(planId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePerformanceReviewPlan(
    [FromBody] CreatePerformanceReviewPlanPayload payload, CancellationToken ct)
    {
        var command =
            new CreatePerformanceReviewPlanCommand(
                payload,
                CurrentUserId
            );

        var result = await Sender.Send(command, ct);

        return HandleResult(result);
    }

    [HttpPut("update/{planId:int}")]
    public async Task<IActionResult> UpdatePerformanceReviewPlanAsync(
    int planId,
    [FromBody] UpdatePerformanceReviewPlanPayload payload,
    CancellationToken ct)
    {
        var command = new UpdatePerformanceReviewPlanCommand(
            planId,
            payload,
            CurrentUserId);

        var result = await Sender.Send(command, ct);

        return HandleResult(result);
    }

    [HttpPost("copy/{planId:int}")]
    public async Task<IActionResult> CopyPerformanceReviewPlanAsync(
    int planId,
    [FromBody] CopyPerformanceReviewPlanPayload payload,
    CancellationToken ct)
    {
        var command = new CopyPerformanceReviewPlanCommand(
            planId,
            payload,
            CurrentUserId);

        var result = await Sender.Send(command, ct);

        return HandleResult(result);
    }

    [HttpDelete("{planId:int}")]
    public async Task<IActionResult> DeletePerformanceReviewPlanAsync(
    int planId,
    CancellationToken ct)
    {
        var command = new DeletePerformanceReviewPlanCommand(
            planId,
            CurrentUserId);

        var result = await Sender.Send(command, ct);

        return HandleResult(result);
    }

    [HttpPost("ongoing/{planId:int}")]
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> ActivatePerformanceReviewPlanAsync(
    int planId,
    CancellationToken ct)
    {
        var command = new ActivatePerformanceReviewPlanCommand(
            planId,
            CurrentUserId);

        var result = await Sender.Send(command, ct);

        return HandleResult(result);
    }


    [HttpGet("{planId:int}/roles")]
    public async Task<IActionResult> GetPlanRolesAsync(int planId, CancellationToken ct)
    {
        var query = new GetPlanRolesQuery(planId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }


}