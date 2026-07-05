namespace HRManagement.Domain.Models.Tables;

public class FillAssignment : BaseTable
{
    public int Id { get; private set; }
    public int PlanId { get; private set; }
    public int IntervalId { get; private set; }
    public int FillerId { get; private set; }
    public int SubjectId { get; private set; }
    public int AssessmentId { get; private set; }
    public string Status { get; private set; } = string.Empty;

    public PerformanceReviewPlanInterval Interval { get; set; } = null!;
    public Employee Filler { get; set; } = null!;
    public Employee Subject { get; set; } = null!;
    public Assessment Assessment { get; set; } = null!;
    public Employee? Modifier { get; set; }

    protected FillAssignment() { }

    public FillAssignment(
        int planId,
        int intervalId,
        int fillerId,
        int subjectId,
        int assessmentId,
        string status,
        int actionerId)
    {
        PlanId = planId;
        IntervalId = intervalId;
        FillerId = fillerId;
        SubjectId = subjectId;
        AssessmentId = assessmentId;
        Status = status;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(string? status, int actionerId)
    {
        Status = UseIfProvided(status, Status);
        MarkAsModified(actionerId);
    }

    public void FinalizeSubmission(bool isDraft, int actionerId)
    {
        string calculatedStatus;

        if (isDraft)
        {
            calculatedStatus = "drafted";
        }
        else
        {
            string answerType = Assessment?.AnswerType?.ToLower() ?? "text";
            calculatedStatus = answerType == "rating" ? "done" : "on review";
        }

        this.ApplyUpdate(calculatedStatus, actionerId);
    }
}