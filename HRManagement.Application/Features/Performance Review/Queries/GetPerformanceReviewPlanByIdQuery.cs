using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Plans.Queries;

public record GetPerformanceReviewPlanByIdQuery(int PlanId)
    : IRequest<Result<PerformanceReviewPlanResponseDto>>;

internal sealed class GetPerformanceReviewPlanByIdQueryHandler(
    IPerformanceReviewPlanRepository planRepository,
    ILogger<GetPerformanceReviewPlanByIdQueryHandler> logger)
    : IRequestHandler<GetPerformanceReviewPlanByIdQuery, Result<PerformanceReviewPlanResponseDto>>
{
    public async Task<Result<PerformanceReviewPlanResponseDto>> Handle(
        GetPerformanceReviewPlanByIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler : {HandlerName} for PlanId: {PlanId}",
            nameof(GetPerformanceReviewPlanByIdQueryHandler),
            request.PlanId);

        var data = await planRepository
            .GetPlanByIdAsync(request.PlanId, cancellationToken);

        if (data is null)
            return Result.Failure<PerformanceReviewPlanResponseDto>("Plan not found");

        return Result.Success(data);
    }
}