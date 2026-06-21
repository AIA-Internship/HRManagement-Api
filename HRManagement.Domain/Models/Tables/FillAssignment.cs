namespace HRManagement.Domain.Models.Tables;

public class FillAssignment : BaseTable
{
    public int Id { get; private set; }
    public int PlanId { get; private set; }
    public int FillerId { get; private set; }
    public int SubjectId { get; private set; }
    public int AssessmentId { get; private set; }
    public string Status { get; private set; } = string.Empty;

    protected FillAssignment() { }

    public FillAssignment(
        int planId,
        int fillerId,
        int subjectId,
        int assessmentId,
        string status,
        int actionerId)
    {
        PlanId = planId;
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
}