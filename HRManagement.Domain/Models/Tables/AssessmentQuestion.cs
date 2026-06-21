namespace HRManagement.Domain.Models.Tables;

public class AssessmentQuestion : BaseTable
{
    public int Id { get; private set; }
    public int AssessmentId { get; private set; }
    public string QuestionText { get; private set; } = string.Empty;
    public int QuestionOrder { get; private set; }

    protected AssessmentQuestion() { }

    public AssessmentQuestion(
        int assessmentId,
        string questionText,
        int questionOrder,
        int actionerId)
    {
        AssessmentId = assessmentId;
        QuestionText = questionText;
        QuestionOrder = questionOrder;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(string? questionText, int? questionOrder, int actionerId)
    {
        QuestionText = UseIfProvided(questionText, QuestionText);
        QuestionOrder = questionOrder ?? QuestionOrder;
        MarkAsModified(actionerId);
    }
}