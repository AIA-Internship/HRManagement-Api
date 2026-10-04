using HRManagement.Application.Features.PerformanceReview.Assignments.Queries;
using HRManagement.Application.Features.PerformanceReview.Commands;
using HRManagement.Application.Features.PerformanceReview.Dashboards.Queries;
using HRManagement.Domain.Models.Payload;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/assignment")]
public class FillAssignmentController(ISender sender) : BaseApiController(sender)
{
    [HttpPost("review/submit")]
    public async Task<IActionResult> SubmitTextReview(
    [FromBody] SubmitTextReviewPayload request,
    CancellationToken cancellationToken)
    {
        var command = new SubmitTextReviewCommand(request, CurrentEmployeeId);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure) return BadRequest(new { Message = result.Error });
        return Ok(new { ReviewAssignmentId = result.Value });
    }

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
        var command = new SaveOrSubmitPeerReviewCommand(intervalId, payload, CurrentEmployeeId);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }

    [HttpPost("assignments/{assignmentId:int}/self-assessment")]
    public async Task<IActionResult> SaveOrSubmitSelfAssessment(
    int assignmentId,
    [FromBody] SaveOrSubmitSelfAssessmentPayload payload,
    CancellationToken ct)
    {
        var command = new SaveOrSubmitSelfAssessmentCommand(assignmentId, payload, CurrentEmployeeId);
        var result = await Sender.Send(command, ct);
        return HandleResult(result);
    }

    [HttpGet("supervisor/dashboard")]
    public async Task<IActionResult> GetSupervisorAssignmentDashboardAsync(
    [FromQuery] int? intervalId,
    CancellationToken ct)
    {
        var query = new GetSupervisorAssignmentDashboardQuery(intervalId, CurrentEmployeeId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpGet("intervals/{intervalId:int}/supervisor-assignments/subjects/{subjectId:int}")]
    public async Task<IActionResult> GetSupervisorAssignmentBySubject(
    [FromRoute] int intervalId,
    [FromRoute] int subjectId,
    [FromQuery] int? reviewerId, // Accepts optional ?reviewerId=... from query string
    CancellationToken ct)
    {
        // If reviewerId is provided and valid (> 0), use it; otherwise fall back to CurrentEmployeeId
        var effectiveSupervisorId = (reviewerId.HasValue && reviewerId.Value > 0)
            ? reviewerId.Value
            : CurrentEmployeeId;

        var query = new GetMySupervisorAssignmentBySubjectQuery(intervalId, effectiveSupervisorId, subjectId);
        var result = await Sender.Send(query, ct);

        return HandleResult(result);
    }

    [HttpPost("supervisor-assessment")]
    public async Task<IActionResult> SaveOrSubmitSupervisorAssessment(
    [FromBody] SaveOrSubmitSupervisorAssessmentPayload request,
    CancellationToken cancellationToken)
    {
        var command = new SaveOrSubmitSupervisorAssessmentCommand(request, CurrentEmployeeId);
        var result = await Sender.Send(command, cancellationToken);
        if (result.IsFailure) return BadRequest(new { Message = result.Error });
        return Ok(new { AssignmentId = result.Value });
    }

    [HttpGet("intervals/{intervalId:int}/self-assessment-review/subject/{subjectId:int}")]
    public async Task<IActionResult> GetSupervisorSelfAssessmentReviewBySubject(
    [FromRoute] int intervalId,
    [FromRoute] int subjectId,
    [FromQuery] int? reviewerId,
    CancellationToken ct)
    {
        // If reviewerId is provided and valid (> 0), use it; otherwise fall back to CurrentEmployeeId
        var effectiveSupervisorId = (reviewerId.HasValue && reviewerId.Value > 0)
            ? reviewerId.Value
            : CurrentEmployeeId;

        var query = new GetSupervisorSelfAssessmentReviewQuery(intervalId, effectiveSupervisorId, subjectId);
        var result = await Sender.Send(query, ct);

        return HandleResult(result);
    }

    [HttpPost("self-assessment/review")]
    public async Task<IActionResult> SaveOrSubmitSelfAssessmentReview(
    [FromBody] SaveOrSubmitSelfAssessmentReviewPayload request,
    CancellationToken cancellationToken)
    {
        var command = new SaveOrSubmitSelfAssessmentReviewCommand(request, CurrentEmployeeId);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Message = result.Error });

        return Ok(new { ReviewAssignmentId = result.Value });
    }

    [HttpGet("intervals/{intervalId:int}/peer-reviews/subjects/{subjectId:int}")]
    public async Task<IActionResult> GetPeerReviewsBySubject(
    [FromRoute] int intervalId,
    [FromRoute] int subjectId,
    CancellationToken ct)
    {
        var query = new GetPeerReviewsBySubjectQuery(CurrentEmployeeId, intervalId, subjectId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }

    [HttpPost("peer-reviews/review")]
    public async Task<IActionResult> SaveOrSubmitPeerReview(
    [FromBody] SaveOrSubmitPeerReviewReviewPayload request,
    CancellationToken cancellationToken)
    {
        var command = new SaveOrSubmitPeerReviewReviewCommand(request, CurrentEmployeeId);
        var result = await Sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Message = result.Error });

        return Ok(new { Message = "Peer review ratings successfully saved." });
    }
}