using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Plans.Queries;

public record GetPerformanceReviewPlansQuery()
    : IRequest<Result<List<PerformanceReviewPlanResponseDto>>>;

internal sealed class GetPerformanceReviewPlansQueryHandler(
    IPerformanceReviewPlanRepository planRepository,
    ILogger<GetPerformanceReviewPlansQueryHandler> logger)
    : IRequestHandler<GetPerformanceReviewPlansQuery, Result<List<PerformanceReviewPlanResponseDto>>>
{
    public async Task<Result<List<PerformanceReviewPlanResponseDto>>> Handle(
        GetPerformanceReviewPlansQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler : {HandlerName}",
            nameof(GetPerformanceReviewPlansQueryHandler));

        var data = await planRepository
            .GetAllPlansAsync(cancellationToken);

        return Result.Success(data);
    }
}