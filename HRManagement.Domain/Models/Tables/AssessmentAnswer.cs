namespace HRManagement.Domain.Models.Tables;

public class AssessmentAnswer : BaseTable
{
    public int Id { get; private set; }
    public int AssignmentId { get; private set; }
    public int AssessmentQuestionId { get; private set; }
    public string? TextValue { get; private set; }
    public int? RatingValue { get; private set; }

    public AssessmentQuestion AssessmentQuestion { get; private set; } = null!;
    public FillAssignment Assignment { get; private set; } = null!;

    protected AssessmentAnswer() { }

    public AssessmentAnswer(
        int assignmentId,
        int assessmentQuestionId,
        string? textValue,
        int? ratingValue,
        int actionerId)
    {
        AssignmentId = assignmentId;
        AssessmentQuestionId = assessmentQuestionId;
        TextValue = textValue;
        RatingValue = ratingValue;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(string? textValue, int? ratingValue, int actionerId)
    {
        TextValue = textValue ?? TextValue;
        RatingValue = ratingValue ?? RatingValue;
        MarkAsModified(actionerId);
    }
}