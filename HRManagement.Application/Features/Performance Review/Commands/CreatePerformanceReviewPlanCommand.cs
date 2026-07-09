using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.SeedWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.Performance_Review.Commands;

public record CreatePerformanceReviewPlanCommand(
    CreatePerformanceReviewPlanPayload Payload,
    int CurrentUserId
) : IRequest<Result>;

internal sealed class CreatePerformanceReviewPlanCommandHandler(
    IPerformanceReviewPlanRepository repository,
    ILogger<CreatePerformanceReviewPlanCommandHandler> logger,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePerformanceReviewPlanCommand, Result>
{
    public async Task<Result> Handle(
        CreatePerformanceReviewPlanCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing handler : {HandlerName}",
            nameof(CreatePerformanceReviewPlanCommandHandler));

        var payload = request.Payload;
        var actionerId = request.CurrentUserId;

        if (!payload.Assessments.Any())
            return Result.Failure("At least one assessment is required.");

        if (!payload.ScoreWeights.Any())
            return Result.Failure("Score weight configuration is required.");

        foreach (var assessment in payload.Assessments)
        {
            if (!assessment.Questions.Any())
                return Result.Failure($"Assessment '{assessment.AssessmentType}' must have at least one question.");

            if (assessment.AssessmentType != "peer-review"
                && !assessment.ReceiverIds.Any())
            {
                return Result.Failure($"{assessment.AssessmentType} must have at least one receiver.");
            }

            if (assessment.AssessmentType == "peer-review"
                && (assessment.Groups == null || !assessment.Groups.Any()))
            {
                return Result.Failure("Peer review must have at least one group.");
            }
        }

        foreach (var group in payload.ScoreWeights.GroupBy(x => x.SubjectRoleId))
        {
            if (group.Sum(x => x.Weight) != 100)
                return Result.Failure(
                    $"Total score weight for role '{group.First().SubjectJobTitle}' must equal 100.");
        }

        await repository.AddPerformanceReviewPlan(
            payload,
            actionerId,
            cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}