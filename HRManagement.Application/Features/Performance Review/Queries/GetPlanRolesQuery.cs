using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReviewPlan.Queries;

public record GetPlanRolesQuery(int PlanId): IRequest<Result<List<string>>>;

internal sealed class GetPlanRolesQueryHandler(IPerformanceReviewPlanRepository repository,ILogger<GetPlanRolesQueryHandler> logger) : IRequestHandler<GetPlanRolesQuery,Result<List<string>>>
{
    public async Task<Result<List<string>>> Handle(
        GetPlanRolesQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler: {HandlerName} for PlanId: {PlanId}",
            nameof(GetPlanRolesQueryHandler),
            request.PlanId);

        var data = await repository.GetPlanRolesAsync(
            request.PlanId,
            cancellationToken);

        return Result.Success(data);
    }
}