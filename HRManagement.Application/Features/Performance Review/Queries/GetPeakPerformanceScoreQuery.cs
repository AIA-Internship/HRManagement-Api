using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetPeakPerformanceScoreQuery(
    long PlanId,
    long InternId)
    : IRequest<Result<InternPerformancePeriodScoreResponseDto>>;

internal sealed class GetPeakPerformanceScoreQueryHandler(
    IInternPerformanceRepository repository,
    ILogger<GetPeakPerformanceScoreQueryHandler> logger)
    : IRequestHandler<
        GetPeakPerformanceScoreQuery,
        Result<InternPerformancePeriodScoreResponseDto>>
{
    public async Task<Result<InternPerformancePeriodScoreResponseDto>> Handle(
        GetPeakPerformanceScoreQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName}, PlanId: {PlanId}, InternId: {InternId}",
            nameof(GetPeakPerformanceScoreQueryHandler),
            request.PlanId,
            request.InternId);

        var data = await repository.GetPeakPerformanceScoreAsync(
            request.PlanId,
            request.InternId,
            cancellationToken);

        return Result.SuccessIf(
            data is not null,
            data!,
            "Peak performance score tidak ditemukan.");
    }
}