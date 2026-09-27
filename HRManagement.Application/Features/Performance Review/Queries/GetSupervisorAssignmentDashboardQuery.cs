using CSharpFunctionalExtensions;
using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Constants;
using HRManagement.Domain.Models.Response;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRManagement.Application.Features.PerformanceReview.Dashboards.Queries;

public record GetSupervisorAssignmentDashboardQuery(
    int? IntervalId,
    int CurrentSupervisorId
) : IRequest<Result<SupervisorDashboardResponseDto>>;

internal sealed class GetSupervisorAssignmentDashboardQueryHandler(
    IAssignmentRepository assignmentRepository,
    ILogger<GetSupervisorAssignmentDashboardQuery> logger)
    : IRequestHandler<GetSupervisorAssignmentDashboardQuery, Result<SupervisorDashboardResponseDto>>
{
    public async Task<Result<SupervisorDashboardResponseDto>> Handle(
        GetSupervisorAssignmentDashboardQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Executing dashboard aggregation for requested Interval ID: {IntervalId} by Supervisor ID: {SupervisorId}",
            request.IntervalId?.ToString() ?? "ONGOING (DEFAULT)",
            request.CurrentSupervisorId);

        // 1. Fetch raw records from repository
        var rawData = await assignmentRepository
            .GetSupervisorAssignmentStatusesAsync(request.IntervalId, cancellationToken);

        if (rawData is null || !rawData.Any())
        {
            logger.LogWarning("No assignment data found for Interval ID: {IntervalId}", request.IntervalId?.ToString() ?? "ONGOING");
            return Result.Success(new SupervisorDashboardResponseDto());
        }

        // 2. Resolve Active PlanId and IntervalId metadata from rawData or request
        // Assumes SupervisorFillAssignmentResponseDto includes PlanId/IntervalId property
        var sampleRecord = rawData.First();
        int? resolvedIntervalId = request.IntervalId ?? sampleRecord.IntervalId;
        int? resolvedPlanId = sampleRecord.PlanId;

        // 3. Perform architectural transformation grouping by Subject
        var dashboardData = rawData
            .GroupBy(x => x.SubjectId)
            .Select(group =>
            {
                var sample = group.First();

                var supervisorAssignment = group.Where(x => x.AssessmentType == "supervisor-assessment").ToList();
                var peerAssignments = group.Where(x => x.AssessmentType == "peer-review").ToList();
                var selfAssignments = group.Where(x => x.AssessmentType == "self-assessment").ToList();

                return new DashboardDataDto
                {
                    SubjectId = group.Key,
                    SubjectName = sample.Subject?.Name ?? string.Empty,
                    JobTitle = sample.Subject?.JobTitle ?? string.Empty,

                    SupervisorAssessment = supervisorAssignment == null || supervisorAssignment.Count <= 0
                        ? null
                        : new AssessmentDetailDto
                        {
                            AssignmentId = supervisorAssignment[0].AssignmentId,
                            AnswerType = supervisorAssignment[0].AnswerType,
                            Status = GetSupervisorAssignmentStatus(supervisorAssignment, request.CurrentSupervisorId),
                            Reviewers = supervisorAssignment[0].Reviewers?
                                .Where(r => r.Id.HasValue)
                                .Select(r => new ReviewerDto
                                {
                                    Id = r.Id!.Value,
                                    Name = r.Name
                                }).ToList() ?? new()
                        },

                    PeerReviews = peerAssignments.Select(p => new AssessmentDetailDto
                    {
                        AssignmentId = p.AssignmentId,
                        AnswerType = p.AnswerType,
                        Status = GetEmployeeFillAssignmentReviewStatus(peerAssignments, request.CurrentSupervisorId),
                        Reviewers = p.Reviewers?
                            .Where(r => r.Id.HasValue)
                            .Select(r => new ReviewerDto { Id = r.Id!.Value, Name = r.Name })
                            .ToList() ?? new()
                    }).ToList(),

                    SelfAssessment = selfAssignments == null || selfAssignments.Count <= 0
                        ? null
                        : new AssessmentDetailDto
                        {
                            AssignmentId = selfAssignments[0].AssignmentId,
                            AnswerType = selfAssignments[0].AnswerType,
                            Status = GetEmployeeFillAssignmentReviewStatus(selfAssignments, request.CurrentSupervisorId),
                            Reviewers = selfAssignments[0].Reviewers?
                                .Where(r => r.Id.HasValue)
                                .Select(r => new ReviewerDto { Id = r.Id!.Value, Name = r.Name })
                                .ToList() ?? new()
                        }
                };
            })
            .ToList();

        // 4. Return wrapped DTO containing metadata and assignment list
        return Result.Success(new SupervisorDashboardResponseDto
        {
            CurrentPlanId = resolvedPlanId,
            CurrentIntervalId = resolvedIntervalId,
            Assignments = dashboardData
        });
    }

    public string GetEmployeeFillAssignmentReviewStatus(List<SupervisorFillAssignmentResponseDto> assignments, int currentSupervisorId)
    {
        if (assignments.Count <= 0)
        {
            return "NO_ASSIGNMENTS";
        }

        if (assignments.All(a => a.AssignmentStatus == AssignmentStatusConstants.Locked))
        {
            return "LOCKED";
        }

        // Flatten all reviewers across all peer review assignments for this subject
        var allReviewers = assignments
            .SelectMany(a => a.Reviewers ?? new List<ReviewerResponseDto>())
            .Where(r => r.Id.HasValue)
            .ToList();

        // If no reviewers exist anywhere, it's not started
        if (allReviewers.Count == 0)
        {
            return "NOT STARTED";
        }

        // Check if ALL assignments are Rating type
        bool isAllRating = assignments.All(a => a.AnswerType == AssignmentAnswerTypeConstants.Rating);

        // Determine overall status flags
        bool allInProgress = assignments.All(a =>
            a.AssignmentStatus == AssignmentStatusConstants.InProgress ||
            a.AssignmentStatus == AssignmentStatusConstants.Drafted ||
            a.AssignmentStatus == AssignmentStatusConstants.OnReview);

        bool allCompleted = assignments.All(a =>
            a.AssignmentStatus == AssignmentStatusConstants.Completed ||
            a.AssignmentStatus == AssignmentStatusConstants.Approved);

        // IN PROGRESS RULES
        if (allInProgress)
        {
            if (isAllRating)
            {
                return "IN PROGRESS";
            }

            // For Text or Mixed, apply the ownership rules
            bool allByMe = allReviewers.All(r => r.Id == currentSupervisorId);
            var distinctOtherReviewerIds = allReviewers.Where(r => r.Id != currentSupervisorId).Select(r => r.Id).Distinct().ToList();
            bool allBySomeoneElseSingle = distinctOtherReviewerIds.Count == 1 && allReviewers.All(r => r.Id != currentSupervisorId);

            if (allByMe)
            {
                return "IN PROGRESS BY ME";
            }
            if (allBySomeoneElseSingle)
            {
                return "IN PROGRESS BY SOMEONE ELSE";
            }
            return "IN PROGRESS";
        }

        // COMPLETED RULES
        if (allCompleted)
        {
            if (isAllRating)
            {
                return "COMPLETED";
            }

            // For Text or Mixed, apply the ownership rules
            bool allByMe = allReviewers.All(r => r.Id == currentSupervisorId);
            var distinctOtherReviewerIds = allReviewers.Where(r => r.Id != currentSupervisorId).Select(r => r.Id).Distinct().ToList();
            bool allBySomeoneElseSingle = distinctOtherReviewerIds.Count == 1 && allReviewers.All(r => r.Id != currentSupervisorId);

            if (allByMe)
            {
                return "COMPLETED BY ME";
            }
            if (allBySomeoneElseSingle)
            {
                return "COMPLETED BY SOMEONE ELSE";
            }
            return "COMPLETED";
        }

        return "";
    }

    public string GetSupervisorAssignmentStatus(List<SupervisorFillAssignmentResponseDto> assignments, int currentSupervisorId)
    {
        if (assignments.Count <= 0)
        {
            return "NO ASSIGNMENTS";
        }

        if (assignments.All(a => a.AssignmentStatus == AssignmentStatusConstants.Locked))
        {
            return "LOCKED";
        }

        // 1. Not Started: If no reviewers exist across all relevant assignments
        if (assignments.All(a => a.Reviewers == null || a.Reviewers.Count <= 0))
        {
            return "NOT STARTED";
        }

        // Flatten all reviewers for easy analysis
        var allReviewers = assignments.SelectMany(a => a.Reviewers ?? new List<ReviewerResponseDto>()).ToList();

        // Check flags for statuses
        bool hasInProgress = assignments.Any(a => a.AssignmentStatus == AssignmentStatusConstants.InProgress ||
                                                  a.AssignmentStatus == AssignmentStatusConstants.Drafted ||
                                                  a.AssignmentStatus == AssignmentStatusConstants.OnReview);

        bool hasCompleted = assignments.Any(a => a.AssignmentStatus == AssignmentStatusConstants.Completed ||
                                                 a.AssignmentStatus == AssignmentStatusConstants.Approved);

        // If at least 1 is in progress and NONE are completed yet
        if (hasInProgress && !hasCompleted)
        {
            bool onlyMe = allReviewers.Count > 0 && allReviewers.All(r => r.Id == currentSupervisorId);
            bool onlyOthers = allReviewers.Count > 0 && allReviewers.All(r => r.Id != currentSupervisorId);
            bool bothMeAndOthers = allReviewers.Any(r => r.Id == currentSupervisorId) && allReviewers.Any(r => r.Id != currentSupervisorId);

            if (onlyMe) return "IN PROGRESS BY ME";
            if (onlyOthers) return "IN PROGRESS BY OTHERS";
            if (bothMeAndOthers) return "IN PROGRESS BY ME AND OTHERS";

            return "IN PROGRESS";
        }

        // If at least 1 is completed
        if (hasCompleted)
        {
            bool onlyMe = allReviewers.Count > 0 && allReviewers.All(r => r.Id == currentSupervisorId);
            bool onlyOthers = allReviewers.Count > 0 && allReviewers.All(r => r.Id != currentSupervisorId);
            bool bothMeAndOthers = allReviewers.Any(r => r.Id == currentSupervisorId) && allReviewers.Any(r => r.Id != currentSupervisorId);

            if (onlyMe) return "COMPLETED BY ME";
            if (onlyOthers) return "COMPLETED BY OTHERS";
            if (bothMeAndOthers) return "COMPLETED BY ME AND OTHERS";

            return "COMPLETED";
        }

        // Fallback if reviewers exist but statuses don't match standard workflow states
        if (allReviewers.All(r => r.Id == currentSupervisorId))
        {
            return "COMPLETED BY ME";
        }
        if (allReviewers.All(r => r.Id != currentSupervisorId))
        {
            return "COMPLETED BY OTHERS";
        }
        if (allReviewers.Any(r => r.Id == currentSupervisorId) && allReviewers.Any(r => r.Id != currentSupervisorId))
        {
            return "COMPLETED BY ME AND OTHERS";
        }

        return "";
    }
}
