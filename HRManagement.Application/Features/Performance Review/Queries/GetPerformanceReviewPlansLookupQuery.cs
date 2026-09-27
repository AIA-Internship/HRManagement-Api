using CSharpFunctionalExtensions;
using HRManagement.Application.Features.Master.References.Queries;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

public record GetPerformanceReviewPlansLookupQuery : IRequest<Result<List<PerformanceReviewPlanLookupDto>>>;

internal sealed class GetPerformanceReviewPlanLookupsQueryHandler(
    IPerformanceReviewPlanRepository planRepository,
    ILogger<GetPerformanceReviewPlanLookupsQueryHandler> logger)
    : IRequestHandler<GetPerformanceReviewPlansLookupQuery, Result<List<PerformanceReviewPlanLookupDto>>>
{
    public async Task<Result<List<PerformanceReviewPlanLookupDto>>> Handle(
        GetPerformanceReviewPlansLookupQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Executing handler : {HandlerName}", nameof(GetPerformanceReviewPlanLookupsQueryHandler));
        var plans = await planRepository.GetPerformanceReviewPlanLookupsAsync(cancellationToken);
        return Result.Success(plans);
    }
}