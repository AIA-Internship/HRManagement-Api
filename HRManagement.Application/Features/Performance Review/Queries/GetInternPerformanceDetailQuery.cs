using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.ESS.Employee.Queries;

public record GetInternPerformanceDetailQuery(
    long PlanId,
    long InternId)
    : IRequest<Result<InternPerformanceDetailResponseDto>>;

internal sealed class GetInternPerformanceDetailQueryHandler(
    IInternPerformanceRepository repository,
    ILogger<GetInternPerformanceDetailQueryHandler> logger)
    : IRequestHandler<
        GetInternPerformanceDetailQuery,
        Result<InternPerformanceDetailResponseDto>>
{
    public async Task<Result<InternPerformanceDetailResponseDto>> Handle(
        GetInternPerformanceDetailQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName}, PlanId: {PlanId}, InternId: {InternId}",
            nameof(GetInternPerformanceDetailQueryHandler),
            request.PlanId,
            request.InternId);

        var data = await repository.GetInternPerformanceDetailAsync(
            request.PlanId,
            request.InternId,
            cancellationToken);

        return Result.SuccessIf(
            data is not null,
            data!,
            "Performance intern tidak ditemukan.");
    }
}