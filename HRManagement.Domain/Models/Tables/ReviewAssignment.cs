namespace HRManagement.Domain.Models.Tables;

public class ReviewAssignment : BaseTable
{
    public int Id { get; private set; }
    public int PlanId { get; private set; }
    public int IntervalId { get; private set; }
    public int AssignmentId { get; private set; }

    public int? ReviewerId { get; private set; }

    public string? ReviewerJobTitle { get; private set; }
    public int? ReviewerRoleId { get; private set; }
    public string Status { get; private set; } = string.Empty;

    public PerformanceReviewPlanInterval Interval { get; private set; } = null!;
    public FillAssignment Assignment { get; private set; } = null!;
    public Roles? ReviewerRole { get; private set; } = null!;
    public Employee? Reviewer { get; private set; } = null!;

    protected ReviewAssignment() { }

    public ReviewAssignment(
        int planId,
        int intervalId,
        int assignmentId,
        int reviewerRoleId,
        string? reviewerJobTitle,
        int? reviewerId,
        string initialStatus,
        int actionerId)
    {
        PlanId = planId;
        IntervalId = intervalId;
        AssignmentId = assignmentId;
        ReviewerRoleId = reviewerRoleId;
        ReviewerJobTitle = reviewerJobTitle;
        ReviewerId = reviewerId;
        Status = initialStatus;

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ClaimBySupervisor(int supervisorId, int actionerId)
    {
        if (ReviewerId.HasValue && ReviewerId.Value != supervisorId)
        {
            throw new InvalidOperationException("This review assignment has already been locked by another supervisor.");
        }

        ReviewerId = supervisorId;
        Status = "Draft";
        MarkAsModified(actionerId);
    }
    public void ApplyUpdate(string? status, int actionerId)
    {
        Status = UseIfProvided(status, Status);
        MarkAsModified(actionerId);
    }
}