namespace HRManagement.Application.Features.PerformanceReview.Queries;

using CSharpFunctionalExtensions;
using MediatR;
using HRManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

public record GetReviewersByEmployeeAndIntervalQuery(int IntervalId, int EmployeeId, string AssessmentType)
    : IRequest<Result<List<ReviewerLookupDto>>>;

internal sealed class GetReviewersByEmployeeAndIntervalQueryHandler(
    IPerformanceReviewPlanRepository performanceReviewRepository,
    ILogger<GetReviewersByEmployeeAndIntervalQueryHandler> logger)
    : IRequestHandler<GetReviewersByEmployeeAndIntervalQuery, Result<List<ReviewerLookupDto>>>
{
    public async Task<Result<List<ReviewerLookupDto>>> Handle(
        GetReviewersByEmployeeAndIntervalQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Fetching reviewers for EmployeeId: {EmployeeId} in IntervalId: {IntervalId} of AssessmentType {AssessmentType}",
            request.EmployeeId, request.IntervalId, request.AssessmentType);

        var reviewers = await performanceReviewRepository.GetSupervisorAssessmentFillerByEmployeeAndIntervalAsync(
            request.IntervalId,
            request.EmployeeId,
            request.AssessmentType,
            cancellationToken);

        return Result.Success(reviewers);
    }
}