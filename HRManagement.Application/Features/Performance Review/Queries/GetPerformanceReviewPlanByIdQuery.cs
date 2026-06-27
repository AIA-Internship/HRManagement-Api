using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using HRManagement.MsSQL.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Plans.Queries;

public record GetPerformanceReviewPlanByIdQuery(int PlanId)
    : IRequest<Result<PerformanceReviewPlanDetailResponseDto>>;

internal sealed class GetPerformanceReviewPlanByIdQueryHandler(
    IPerformanceReviewPlanRepository planRepository,
    ILogger<GetPerformanceReviewPlanByIdQueryHandler> logger)
    : IRequestHandler<
        GetPerformanceReviewPlanByIdQuery,
        Result<PerformanceReviewPlanDetailResponseDto>>
{

    public async Task<Result<PerformanceReviewPlanDetailResponseDto>> Handle(
    GetPerformanceReviewPlanByIdQuery request,
    CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler : {HandlerName} for PlanId: {PlanId}",
            nameof(GetPerformanceReviewPlanByIdQueryHandler),
            request.PlanId);

        var data = await planRepository
            .GetPlanByIdAsync(
                request.PlanId,
                cancellationToken);

        if (data is null)
            return Result.Failure<PerformanceReviewPlanDetailResponseDto>(
                "Performance review plan not found");

        return Result.Success(data);
    }
}