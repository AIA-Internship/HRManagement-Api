using HRManagement.Application.Auth.Permissions;
using HRManagement.Application.Features.PerformanceReview.AssessmentQuestions.Queries;
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
public class AssessmentController(ISender sender) : BaseApiController(sender)
{
    [HttpGet("assessment/{assessmentId}/questions")]
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetAssessmentQuestionsAsync(
    int assessmentId,
    CancellationToken ct)
    {
        var query = new GetAssessmentQuestionsByAssessmentIdQuery(assessmentId);
        var result = await Sender.Send(query, ct);
        return HandleResult(result);
    }
}