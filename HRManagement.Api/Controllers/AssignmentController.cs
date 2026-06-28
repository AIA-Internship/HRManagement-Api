using HRManagement.Application.Features.PerformanceReview.Assignments.Queries;
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
    [HttpGet("{assignmentId}")]
    public async Task<IActionResult> GetAssignmentDetailByIdAsync(int assignmentId, CancellationToken ct)
    {
        var query = new GetAssignmentDetailByIdQuery(assignmentId, CurrentEmployeeId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }
}