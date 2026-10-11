using HRManagement.Domain.Models.Constants;

namespace HRManagement.Domain.Models.Tables;

public class FillAssignment : BaseTable
{
    public int Id { get; private set; }
    public int PlanId { get; private set; }
    public int IntervalId { get; private set; }
    public int FillerId { get; private set; }
    public int SubjectId { get; private set; }
    public int AssessmentId { get; private set; }
    public string Status { get; private set; } = AssignmentStatusConstant.NotStarted;

    public PerformanceReviewPlanInterval Interval { get; set; } = null!;
    public Employee Filler { get; set; } = null!;
    public Employee Subject { get; set; } = null!;
    public Assessment Assessment { get; set; } = null!;
    public Employee? Modifier { get; set; }

    public ICollection<ReviewAssignment> ReviewAssignments { get; set; } = new List<ReviewAssignment>();

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
        Status = string.IsNullOrWhiteSpace(status) ? AssignmentStatusConstant.NotStarted : status;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(string? status, int actionerId)
    {
        Status = UseIfProvided(status, Status);
        MarkAsModified(actionerId);
    }

    public void FinalizeSubmission(bool isDraft, string? assessmentType, string? answerType, int actionerId)
    {
        string calculatedStatus;

        if (isDraft)
        {
            calculatedStatus = AssignmentStatusConstant.InProgress;
        }
        else
        {
            string normAssessmentType = assessmentType?.ToLower() ?? string.Empty;
            string normAnswerType = answerType?.ToLower() ?? AssessmentAnswerTypeConstants.Text;

            // Supervisor assessment submissions are immediately finalized/completed
            if (normAssessmentType == AssessmentTypeConstant.SupervisorAssessment.ToLower())
            {
                calculatedStatus = AssignmentStatusConstant.Completed;
            }
            else
            {
                // Self and Peer assessments go directly to Completed for ratings or PendingReview for text feedback
                calculatedStatus = normAnswerType == AssessmentAnswerTypeConstants.Rating
                    ? AssignmentStatusConstant.Completed
                    : AssignmentStatusConstant.PendingReview;
            }
        }

        this.ApplyUpdate(calculatedStatus, actionerId);
    }
}