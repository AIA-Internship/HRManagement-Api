using HRManagement.Application.Features.PerformanceReview.Plans.Queries;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRManagement.Application.Features.Performance_Review.Commands;
using HRManagement.Application.Auth.Permissions;

namespace HRManagement.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/plan")]
public class PerformanceReviewPlanController(ISender sender) : BaseApiController(sender)
{

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

    [HttpPost("create")]
    //[HasPermission(Permissions.Users.View)]
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
}