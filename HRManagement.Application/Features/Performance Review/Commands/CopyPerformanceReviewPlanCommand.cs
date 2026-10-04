using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Application.Features.Performance_Review.Commands
{
    public record CopyPerformanceReviewPlanCommand(
        int PlanId,
        CopyPerformanceReviewPlanPayload Payload,
        int CurrentUserId
    ) : IRequest<Result>;

    internal sealed class CopyPerformanceReviewPlanCommandHandler(
    IPerformanceReviewPlanRepository repository,
    ILogger<CopyPerformanceReviewPlanCommandHandler> logger,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CopyPerformanceReviewPlanCommand, Result>
    {
        public async Task<Result> Handle(
            CopyPerformanceReviewPlanCommand request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Executing handler : {HandlerName}",
                nameof(CopyPerformanceReviewPlanCommandHandler));

            if (string.IsNullOrWhiteSpace(request.Payload.Name))
                return Result.Failure("Plan name is required.");

            if (request.Payload.StartDate >= request.Payload.EndDate)
                return Result.Failure("Start date must be before end date.");

            if (request.Payload.DurationInMonth <= 0)
                return Result.Failure("Duration must be greater than 0.");

            await repository.CopyPerformanceReviewPlan(
                request.PlanId,
                request.Payload,
                request.CurrentUserId,
                cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }
}
