using HRManagement.Application.Auth.Permissions;
using HRManagement.Application.Features.PerformanceReview.Plans.Queries;
using HRManagement.Application.Features.PerformanceReview.ScoreWeights.Queries;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/plan")]
public class PerformanceReviewPlanController(ISender sender) : BaseApiController(sender)
{
    [HttpGet("{planId}/score-weights")]
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetScoreWeightsAsync(int planId, [FromQuery] string jobTitle, CancellationToken ct)
    {
        var query = new GetPlanScoreWeightsByPlanIdQuery(planId, jobTitle);
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

    [HttpGet]
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetAllPlansAsync(CancellationToken ct)
    {
        var query = new GetPerformanceReviewPlansQuery();
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }
}