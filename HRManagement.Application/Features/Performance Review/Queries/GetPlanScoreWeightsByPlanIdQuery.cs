using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.ScoreWeights.Queries;

public record GetPlanScoreWeightsByPlanIdQuery(int PlanId, string? JobTitle) : IRequest<Result<List<PlanScoreWeightResponseDto>>>;

internal sealed class GetPlanScoreWeightsQueryHandler(
    IPlanScoreWeightRepository planScoreWeightRepository,
    ILogger<GetPlanScoreWeightsQueryHandler> logger) : IRequestHandler<GetPlanScoreWeightsByPlanIdQuery, Result<List<PlanScoreWeightResponseDto>>>
{
    public async Task<Result<List<PlanScoreWeightResponseDto>>> Handle(
        GetPlanScoreWeightsByPlanIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler : {HandlerName} for PlanId: {PlanId}, JobTitle: {JobTitle}",
            nameof(GetPlanScoreWeightsQueryHandler),
            request.PlanId,
            request.JobTitle);

        var data = await planScoreWeightRepository
            .GetByPlanIdAndJobTitleAsync(
                request.PlanId,
                request.JobTitle,
                cancellationToken);

        return Result.Success(data);
    }
}