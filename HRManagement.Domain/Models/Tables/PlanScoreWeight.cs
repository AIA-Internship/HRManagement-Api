namespace HRManagement.Domain.Models.Tables;

public class PlanScoreWeight : BaseTable
{
    public int Id { get; private set; }
    public int PlanId { get; private set; }
    public int? SubjectRoleId { get; private set; }
    public string? SubjectJobTitle { get; private set; }
    public string ScoreType { get; private set; } = string.Empty;
    public decimal Weights { get; private set; }

    public PerformanceReviewPlan Plan { get; private set; } = null!;
    protected PlanScoreWeight() { }

    public PlanScoreWeight(
        int planId,
        int? subjectRoleId,
        string? subjectJobTitle,
        string scoreType,
        decimal weights,
        int actionerId)
    {
        PlanId = planId;
        SubjectRoleId = subjectRoleId;
        SubjectJobTitle = subjectJobTitle;
        ScoreType = scoreType;
        Weights = weights;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(
        string? scoreType,
        decimal? weights,
        int? subjectRoleId,
        string? subjectJobTitle,
        int actionerId)
    {
        ScoreType = UseIfProvided(scoreType, ScoreType);
        Weights = weights ?? Weights;
        SubjectRoleId = subjectRoleId ?? SubjectRoleId;
        SubjectJobTitle = UseIfProvided(subjectJobTitle, SubjectJobTitle ?? "");
        MarkAsModified(actionerId);
    }
}