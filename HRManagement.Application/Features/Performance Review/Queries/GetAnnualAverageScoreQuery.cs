using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetAnnualAverageScoreQuery(
    long PlanId,
    long InternId)
    : IRequest<Result<InternPerformanceAnnualAverageResponseDto>>;

internal sealed class GetAnnualAverageScoreQueryHandler(
    IInternPerformanceRepository repository,
    ILogger<GetAnnualAverageScoreQueryHandler> logger)
    : IRequestHandler<
        GetAnnualAverageScoreQuery,
        Result<InternPerformanceAnnualAverageResponseDto>>
{
    public async Task<Result<InternPerformanceAnnualAverageResponseDto>> Handle(
        GetAnnualAverageScoreQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName}, PlanId: {PlanId}, InternId: {InternId}",
            nameof(GetAnnualAverageScoreQueryHandler),
            request.PlanId,
            request.InternId);

        var data = await repository.GetAnnualAverageScoreAsync(
            request.PlanId,
            request.InternId,
            cancellationToken);

        return Result.SuccessIf(
            data is not null,
            data!,
            "Annual average score tidak ditemukan.");
    }
}