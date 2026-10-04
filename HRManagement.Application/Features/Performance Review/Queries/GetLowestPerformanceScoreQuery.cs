using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetLowestPerformanceScoreQuery(
    long PlanId,
    long InternId)
    : IRequest<Result<InternPerformancePeriodScoreResponseDto>>;

internal sealed class GetLowestPerformanceScoreQueryHandler(
    IInternPerformanceRepository repository,
    ILogger<GetLowestPerformanceScoreQueryHandler> logger)
    : IRequestHandler<
        GetLowestPerformanceScoreQuery,
        Result<InternPerformancePeriodScoreResponseDto>>
{
    public async Task<Result<InternPerformancePeriodScoreResponseDto>> Handle(
        GetLowestPerformanceScoreQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName}, PlanId: {PlanId}, InternId: {InternId}",
            nameof(GetLowestPerformanceScoreQueryHandler),
            request.PlanId,
            request.InternId);

        var data = await repository.GetLowestPerformanceScoreAsync(
            request.PlanId,
            request.InternId,
            cancellationToken);

        return Result.SuccessIf(
            data is not null,
            data!,
            "Lowest performance score tidak ditemukan.");
    }
}