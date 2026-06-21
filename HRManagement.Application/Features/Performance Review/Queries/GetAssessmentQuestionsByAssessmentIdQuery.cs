using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.AssessmentQuestions.Queries;

public record GetAssessmentQuestionsByAssessmentIdQuery(int AssessmentId)
    : IRequest<Result<List<AssessmentQuestionResponseDto>>>;

internal sealed class GetAssessmentQuestionsQueryHandler(
    IAssessmentQuestionRepository repository,
    ILogger<GetAssessmentQuestionsQueryHandler> logger)
    : IRequestHandler<GetAssessmentQuestionsByAssessmentIdQuery, Result<List<AssessmentQuestionResponseDto>>>
{
    public async Task<Result<List<AssessmentQuestionResponseDto>>> Handle(
        GetAssessmentQuestionsByAssessmentIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {Handler} for AssessmentId: {AssessmentId}",
            nameof(GetAssessmentQuestionsQueryHandler),
            request.AssessmentId);

        var data = await repository.GetByAssessmentIdAsync(
            request.AssessmentId,
            cancellationToken);

        return Result.Success(data);
    }
}