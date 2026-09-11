using HRManagement.Application.Features.ESS.Employee.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/intern-performance")]
public class InternPerformanceController(ISender sender) : BaseApiController(sender)
{
    [HttpGet("{planId:long}/ranking")]
    public async Task<IActionResult> GetInternPerformanceRankingAsync(long planId, [FromQuery] string role, CancellationToken ct)
    {
        var query = new GetInternPerformanceRankingQuery(planId,role);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("{planId:long}/{internId:long}/detail")]
    public async Task<IActionResult> GetInternPerformanceDetailAsync(long planId, long internId, CancellationToken ct)
    {
        var query = new GetInternPerformanceDetailQuery(planId, internId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("{planId:long}/{internId:long}/peak")]
    public async Task<IActionResult> GetPeakPerformanceScoreAsync(long planId, long internId, CancellationToken ct)
    {
        var query = new GetPeakPerformanceScoreQuery(planId, internId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("{planId:long}/{internId:long}/lowest")]
    public async Task<IActionResult> GetLowestPerformanceScoreAsync(long planId, long internId, CancellationToken ct)
    {
        var query = new GetLowestPerformanceScoreQuery(planId, internId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("{planId:long}/{internId:long}/annual-average")]
    public async Task<IActionResult> GetAnnualAverageScoreAsync(long planId, long internId, CancellationToken ct)
    {
        var query = new GetAnnualAverageScoreQuery(planId, internId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

}