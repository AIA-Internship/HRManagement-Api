namespace HRManagement.Domain.Models.Tables;

public class Assessment : BaseTable
{
    public int Id { get; private set; }
    public int PlanId { get; private set; }
    public string AnswerType { get; private set; } = string.Empty;
    public string AssessmentType { get; private set; } = string.Empty;
    public int? FillerRoleId { get; private set; }
    public string? FillerJobTitle { get; private set; }
    public int? SubjectRoleId { get; private set; }
    public string? SubjectJobTitle { get; private set; }

    protected Assessment() { }

    public Assessment(
        int planId,
        string answerType,
        string assessmentType,
        int? fillerRoleId,
        string? fillerJobTitle,
        int? subjectRoleId,
        string? subjectJobTitle,
        int actionerId)
    {
        PlanId = planId;
        AnswerType = answerType;
        AssessmentType = assessmentType;
        FillerRoleId = fillerRoleId;
        FillerJobTitle = fillerJobTitle;
        SubjectRoleId = subjectRoleId;
        SubjectJobTitle = subjectJobTitle;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(
        string? answerType,
        string? assessmentType,
        int? fillerRoleId,
        string? fillerJobTitle,
        int? subjectRoleId,
        string? subjectJobTitle,
        int actionerId)
    {
        AnswerType = UseIfProvided(answerType, AnswerType);
        AssessmentType = UseIfProvided(assessmentType, AssessmentType);
        FillerRoleId = fillerRoleId ?? FillerRoleId;
        FillerJobTitle = UseIfProvided(fillerJobTitle, FillerJobTitle ?? "");
        SubjectRoleId = subjectRoleId ?? SubjectRoleId;
        SubjectJobTitle = UseIfProvided(subjectJobTitle, SubjectJobTitle ?? "");
        MarkAsModified(actionerId);
    }
}