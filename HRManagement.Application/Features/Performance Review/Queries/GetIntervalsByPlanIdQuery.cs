using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetIntervalsByPlanIdQuery(int PlanId) : IRequest<Result<List<PerformanceReviewIntervalLookupDto>>>;

internal sealed class GetIntervalsByPlanIdQueryHandler(
    IPerformanceReviewPlanRepository planRepository,
    ILogger<GetIntervalsByPlanIdQueryHandler> logger)
    : IRequestHandler<GetIntervalsByPlanIdQuery, Result<List<PerformanceReviewIntervalLookupDto>>>
{
    public async Task<Result<List<PerformanceReviewIntervalLookupDto>>> Handle(
        GetIntervalsByPlanIdQuery request,
        CancellationToken cancellationToken)
    {
        var intervals = await planRepository.GetIntervalsByPlanIdAsync(request.PlanId, cancellationToken);
        return Result.Success(intervals);
    }
}