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

    public string? RatingDescription { get; private set; }

    public PerformanceReviewPlan Plan { get; private set; } = null!;
    public ICollection<AssessmentGroup> Groups { get; private set; } = new List<AssessmentGroup>();
    public ICollection<AssessmentQuestion> Questions { get; private set; } = new List<AssessmentQuestion>();

    public ICollection<AssessmentReceiver> Receivers { get; set; } = new List<AssessmentReceiver>();

    protected Assessment() { }

    public Assessment(
        int planId,
        string answerType,
        string assessmentType,
        int? fillerRoleId,
        string? fillerJobTitle,
        int? subjectRoleId,
        string? subjectJobTitle,
        int actionerId,
        string? ratingDescription)
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
        RatingDescription = ratingDescription;
    }

    public void ApplyUpdate(
        string? answerType,
        string? assessmentType,
        int? fillerRoleId,
        string? fillerJobTitle,
        int? subjectRoleId,
        string? subjectJobTitle,
        int actionerId,
        string? ratingDescription)
    {
        AnswerType = UseIfProvided(answerType, AnswerType);
        AssessmentType = UseIfProvided(assessmentType, AssessmentType);
        FillerRoleId = fillerRoleId ?? FillerRoleId;
        FillerJobTitle = UseIfProvided(fillerJobTitle, FillerJobTitle ?? "");
        SubjectRoleId = subjectRoleId ?? SubjectRoleId;
        SubjectJobTitle = UseIfProvided(subjectJobTitle, SubjectJobTitle ?? "");
        RatingDescription = UseIfProvided(ratingDescription, RatingDescription ?? "");
        MarkAsModified(actionerId);
    }
}