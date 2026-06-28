namespace HRManagement.Domain.Models.Tables;

public class PerformanceReviewPlan : BaseTable
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string PeriodType { get; private set; } = string.Empty;
    public int DurationInMonth { get; private set; }
    public int MinReviewDurationInDays { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Status { get; private set; } = string.Empty;

    public ICollection<PerformanceReviewPlanInterval> Intervals { get; private set; } = new List<PerformanceReviewPlanInterval>();
    public ICollection<PerformanceReviewPlanScoreWeight> PerformanceReviewPlanScoreWeights { get; private set; } = new List<PerformanceReviewPlanScoreWeight>();
    public ICollection<Assessment> Assessments { get; private set; } = new List<Assessment>();
    public ICollection<FillAssignment> FillAssignments { get; private set; } = new List<FillAssignment>();

    protected PerformanceReviewPlan() { }

    public PerformanceReviewPlan(
        string name,
        string periodType,
        int durationInMonth,
        int minReviewDurationInDays,
        DateTime startDate,
        DateTime endDate,
        string status,
        int actionerId)
    {
        Name = name;
        PeriodType = periodType;
        DurationInMonth = durationInMonth;
        MinReviewDurationInDays = minReviewDurationInDays;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate()
    {
        // ... update properties as needed
        MarkAsModified(1); // Replace 1 with the actual actionerId
    }
}