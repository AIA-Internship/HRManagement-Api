using HRManagement.Application.Auth.Permissions;
using HRManagement.Application.Features.ESS.Employee.Commands;
using HRManagement.Application.Features.PerformanceReview.Assignments.Queries;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Payload;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/assignment")]
public class FillAssignmentController(ISender sender) : BaseApiController(sender)
{
    [HttpGet("{assignmentId:int}")]
    public async Task<IActionResult> GetAssignmentDetailByIdAsync(
    [FromRoute] int assignmentId, 
    CancellationToken ct)
    {
        var query = new GetAssignmentDetailByIdQuery(assignmentId, CurrentEmployeeId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("intervals/{intervalId:int}/peer-reviews")]
    public async Task<IActionResult> GetMyPeerReviewsByInterval(
    [FromRoute] int intervalId,
    CancellationToken ct)
    {
        var query = new GetMyPeerAssignmentsByIntervalQuery(intervalId, CurrentEmployeeId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpPost("intervals/{intervalId:int}/peer-reviews")]
    public async Task<IActionResult> SaveOrSubmitPeerReviews(
    int intervalId,
    [FromBody] SaveOrSubmitPeerReviewPayload payload,
    CancellationToken ct)
    {
        var command = new SaveOrSubmitPeerReviewCommand(intervalId, payload, CurrentUserId);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }
}