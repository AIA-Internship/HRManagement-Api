using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRManagement.Application.Features.Performance_Review.Commands;

    public record DeletePerformanceReviewPlanCommand(
        int PlanId,
        int CurrentUserId
    ) : IRequest<Result>;

    internal sealed class DeletePerformanceReviewPlanCommandHandler(
    IPerformanceReviewPlanRepository repository,
    ILogger<DeletePerformanceReviewPlanCommandHandler> logger,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePerformanceReviewPlanCommand, Result>
    {
        public async Task<Result> Handle(
            DeletePerformanceReviewPlanCommand request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Executing handler : {HandlerName}",
                nameof(DeletePerformanceReviewPlanCommandHandler));

            await repository.DeletePerformanceReviewPlan(
                request.PlanId,
                request.CurrentUserId,
                cancellationToken);

            await unitOfWork.CommitAsync(cancellationToken);

            return Result.Success();
        }
    }

