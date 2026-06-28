using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Plans.Queries;

public record GetEmployeeOngoingPerformanceReviewPlanQuery(
    int fillerId
    ) : IRequest<Result<EmployeeOngoingPerformanceReviewPlanResponseDto>>;

internal sealed class GetOngoingPerformanceReviewPlanQueryHandler(
    IPerformanceReviewPlanRepository planRepository,
    ILogger<GetOngoingPerformanceReviewPlanQueryHandler> logger)
    : IRequestHandler<GetEmployeeOngoingPerformanceReviewPlanQuery,
        Result<EmployeeOngoingPerformanceReviewPlanResponseDto>>
{
    public async Task<Result<EmployeeOngoingPerformanceReviewPlanResponseDto>> Handle(
        GetEmployeeOngoingPerformanceReviewPlanQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName}",
            nameof(GetOngoingPerformanceReviewPlanQueryHandler));

        var data = await planRepository
            .GetEmployeeOngoingPerformanceReviewPlanAsync(request.fillerId, cancellationToken);

        return Result.SuccessIf(
            data is not null,
            data!,
            "Plan tidak ditemukan. Saat ini tidak ada plan yang sedang berlangsung...");
    }
}