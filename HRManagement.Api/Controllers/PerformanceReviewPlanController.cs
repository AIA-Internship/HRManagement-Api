using HRManagement.Application.Auth.Permissions;
using HRManagement.Application.Features.PerformanceReview.Plans.Queries;
using HRManagement.Application.Features.PerformanceReview.Queries;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/plan")]
[Route("api/plans")]
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
    //[HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetPlanByIdAsync(int planId, CancellationToken ct)
    {
        var query = new GetPerformanceReviewPlanByIdQuery(planId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet]
    //[HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetAllPlansAsync(CancellationToken ct)
    {
        var query = new GetPerformanceReviewPlansQuery();
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("ongoing")]
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
}