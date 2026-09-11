using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.Performance_Review.Commands;

public record ActivatePerformanceReviewPlanCommand(
    int PlanId,
    int CurrentUserId
) : IRequest<Result>;

internal sealed class ActivatePerformanceReviewPlanCommandHandler(
    IPerformanceReviewPlanRepository repository,
    ILogger<ActivatePerformanceReviewPlanCommandHandler> logger,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ActivatePerformanceReviewPlanCommand, Result>
{
    public async Task<Result> Handle(
        ActivatePerformanceReviewPlanCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler : {HandlerName} for PlanId: {PlanId}",
            nameof(ActivatePerformanceReviewPlanCommandHandler),
            request.PlanId);

        var success = await repository.ActivatePerformanceReviewPlan(
            request.PlanId,
            request.CurrentUserId,
            cancellationToken);

        if (!success)
        {
            return Result.Failure(
                "Performance review plan gagal diaktifkan.");
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}