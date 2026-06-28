namespace HRManagement.Domain.Models.Tables;

public class PerformanceReviewPlanInterval : BaseTable
{
    public int Id { get; private set; }
    public int PlanId { get; private set; }
    public int IntervalNumber { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Status { get; private set; } = string.Empty;

    public PerformanceReviewPlan PerformanceReviewPlan { get; private set; } = null!;

    protected PerformanceReviewPlanInterval() { }

    public PerformanceReviewPlanInterval(
        int planId,
        int intervalNumber,
        DateTime startDate,
        DateTime dueDate,
        DateTime endDate,
        string status,
        int actionerId)
    {
        PlanId = planId;
        IntervalNumber = intervalNumber;
        StartDate = startDate;
        DueDate = dueDate;
        EndDate = endDate;
        Status = status;

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void UpdateIntervalStatus(string newStatus, int actionerId)
    {
        Status = newStatus;
        MarkAsModified(actionerId);
    }
}