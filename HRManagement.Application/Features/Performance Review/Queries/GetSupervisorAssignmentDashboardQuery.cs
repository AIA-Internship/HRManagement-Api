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
        var sampleRecord = rawData.First();
        int? resolvedIntervalId = request.IntervalId ?? sampleRecord.IntervalId;
        int? resolvedPlanId = sampleRecord.PlanId;

        // 3. Perform architectural transformation grouping by Subject
        var dashboardData = rawData
            .GroupBy(x => x.SubjectId)
            .Select(group =>
            {
                var sample = group.First();

                var supervisorAssignment = group.Where(x => x.AssessmentType == AssessmentTypeConstant.SupervisorAssessment).ToList();
                var peerAssignments = group.Where(x => x.AssessmentType == AssessmentTypeConstant.PeerReview).ToList();
                var selfAssignments = group.Where(x => x.AssessmentType == AssessmentTypeConstant.SelfAssessment).ToList();

                // Determine single employee-level status
                string unifiedStatus = GetUnifiedEmployeeStatus(supervisorAssignment, peerAssignments, selfAssignments);

                return new DashboardDataDto
                {
                    SubjectId = group.Key,
                    SubjectName = sample.Subject?.Name ?? string.Empty,
                    JobTitle = sample.Subject?.JobTitle ?? string.Empty,
                    Status = unifiedStatus,

                    SupervisorAssessment = supervisorAssignment.Count == 0 ? null : new AssessmentDetailDto
                    {
                        AssignmentId = supervisorAssignment[0].AssignmentId,
                        AnswerType = supervisorAssignment[0].AnswerType,
                        Status = GetAssignmentStatusLabel(supervisorAssignment),
                        Reviewers = supervisorAssignment[0].Reviewers?
                            .Where(r => r.Id.HasValue)
                            .Select(r => new ReviewerDto { Id = r.Id!.Value, Name = r.Name })
                            .ToList() ?? new()
                    },

                    PeerReviews = peerAssignments.Select(p => new AssessmentDetailDto
                    {
                        AssignmentId = p.AssignmentId,
                        AnswerType = p.AnswerType,
                        Status = GetAssignmentStatusLabel(new List<SupervisorFillAssignmentResponseDto> { p }),
                        Reviewers = p.Reviewers?
                            .Where(r => r.Id.HasValue)
                            .Select(r => new ReviewerDto { Id = r.Id!.Value, Name = r.Name })
                            .ToList() ?? new()
                    }).ToList(),

                    SelfAssessment = selfAssignments.Count == 0 ? null : new AssessmentDetailDto
                    {
                        AssignmentId = selfAssignments[0].AssignmentId,
                        AnswerType = selfAssignments[0].AnswerType,
                        Status = GetAssignmentStatusLabel(selfAssignments),
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

    public string GetUnifiedEmployeeStatus(
    List<SupervisorFillAssignmentResponseDto> supervisorAssignments,
    List<SupervisorFillAssignmentResponseDto> peerAssignments,
    List<SupervisorFillAssignmentResponseDto> selfAssignments)
    {
        var allAssignments = supervisorAssignments
            .Concat(peerAssignments)
            .Concat(selfAssignments)
            .ToList();

        // 1. If absolutely no assessment records exist yet anywhere -> NOT STARTED
        if (allAssignments.Count == 0)
        {
            return "NOT STARTED";
        }

        // 2. If ANY existing assignment is locked -> LOCKED
        if (allAssignments.Any(a => IsStatus(a.AssignmentStatus, AssignmentStatusConstant.Locked)))
        {
            return "LOCKED";
        }

        // 3. If ALL existing rows are untouched ("Not Started") -> NOT STARTED
        // (Even if self-assessment exists, if it and any on-demand records are NotStarted, the whole thing is Not Started)
        if (allAssignments.All(a => IsStatus(a.AssignmentStatus, AssignmentStatusConstant.NotStarted)))
        {
            return "NOT STARTED";
        }

        // 4. If ANY existing assignment has active work (In Progress, Pending Review, On Review) -> IN PROGRESS
        bool hasActiveWork = allAssignments.Any(a =>
            IsStatus(a.AssignmentStatus, AssignmentStatusConstant.InProgress) ||
            IsStatus(a.AssignmentStatus, AssignmentStatusConstant.PendingReview) ||
            IsStatus(a.AssignmentStatus, AssignmentStatusConstant.OnReview));

        if (hasActiveWork)
        {
            return "IN PROGRESS";
        }

        // 5. If everything that has been instantiated so far is completed or reviewed -> COMPLETED
        bool allCompleted = allAssignments.All(a =>
            IsStatus(a.AssignmentStatus, AssignmentStatusConstant.Completed) ||
            IsStatus(a.AssignmentStatus, AssignmentStatusConstant.Reviewed));

        if (allCompleted)
        {
            return "COMPLETED";
        }

        // Fallback default for mixed/partial states
        return "IN PROGRESS";
    }

    public string GetAssignmentStatusLabel(List<SupervisorFillAssignmentResponseDto> assignments)
    {
        if (assignments.Count <= 0)
        {
            return "NOT STARTED";
        }

        if (assignments.All(a => IsStatus(a.AssignmentStatus, AssignmentStatusConstant.Locked)))
        {
            return "LOCKED";
        }

        if (assignments.All(a => IsStatus(a.AssignmentStatus, AssignmentStatusConstant.NotStarted)))
        {
            return "NOT STARTED";
        }

        if (assignments.Any(a => IsStatus(a.AssignmentStatus, AssignmentStatusConstant.InProgress)))
        {
            return "IN PROGRESS";
        }

        if (assignments.Any(a => IsStatus(a.AssignmentStatus, AssignmentStatusConstant.PendingReview)))
        {
            return "PENDING REVIEW";
        }

        if (assignments.Any(a => IsStatus(a.AssignmentStatus, AssignmentStatusConstant.OnReview)))
        {
            return "ON REVIEW";
        }

        if (assignments.All(a => IsStatus(a.AssignmentStatus, AssignmentStatusConstant.Reviewed)))
        {
            return "REVIEWED";
        }

        if (assignments.All(a => IsStatus(a.AssignmentStatus, AssignmentStatusConstant.Completed)))
        {
            return "COMPLETED";
        }

        return "IN PROGRESS";
    }

    private bool IsStatus(object? statusValue, string targetStatus)
    {
        if (statusValue == null) return false;

        if (statusValue is string strVal)
        {
            // Case-insensitive comparison and handling spaces/underscores robustly
            return string.Equals(strVal, targetStatus, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(strVal.Replace(" ", ""), targetStatus.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(strVal.Replace("_", " "), targetStatus, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}