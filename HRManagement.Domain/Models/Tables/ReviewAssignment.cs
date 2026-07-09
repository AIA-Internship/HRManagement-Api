namespace HRManagement.Domain.Models.Tables;

public class ReviewAssignment : BaseTable
{
    public int Id { get; private set; }
    public int PlanId { get; private set; }
    public int IntervalId { get; private set; }
    public int AssignmentId { get; private set; }
    public int ReviewerId { get; private set; }
    public string Status { get; private set; } = string.Empty;

    public FillAssignment Assignment { get; private set; } = null!;

    protected ReviewAssignment() { }

    public ReviewAssignment(
        int planId,
        int intervalId,
        int assignmentId,
        int reviewerId,
        string status,
        int actionerId)
    {
        PlanId = planId;
        IntervalId = intervalId;
        AssignmentId = assignmentId;
        ReviewerId = reviewerId;
        Status = status;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(string? status, int actionerId)
    {
        Status = UseIfProvided(status, Status);
        MarkAsModified(actionerId);
    }
}