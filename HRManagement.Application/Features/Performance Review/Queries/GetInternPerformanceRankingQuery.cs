using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetInternPerformanceRankingQuery(long PlanId,string role)
    : IRequest<Result<List<InternPerformanceRankingResponseDto>>>;

internal sealed class GetInternPerformanceRankingQueryHandler(
    IInternPerformanceRepository repository,
    ILogger<GetInternPerformanceRankingQueryHandler> logger)
    : IRequestHandler<
        GetInternPerformanceRankingQuery,
        Result<List<InternPerformanceRankingResponseDto>>>
{
    public async Task<Result<List<InternPerformanceRankingResponseDto>>> Handle(
        GetInternPerformanceRankingQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName} for PlanId: {PlanId}",
            nameof(GetInternPerformanceRankingQueryHandler),
            request.PlanId);

        var data = await repository.GetInternPerformanceRankingAsync(
            request.PlanId,
            request.role,
            cancellationToken);

        return Result.Success(data);
    }
}