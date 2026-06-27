namespace HRManagement.Domain.Models.Tables;

public class AssessmentQuestion : BaseTable
{
    public int Id { get; private set; }
    public int AssessmentId { get; private set; }
    public string QuestionText { get; private set; } = string.Empty;
    public int QuestionOrder { get; private set; }

    public string QuestionType { get; private set; } = string.Empty;

    public Assessment Assessment { get; private set; }

    public ICollection<AssessmentAnswer> Answers { get; private set; }
    =new List<AssessmentAnswer>();

    protected AssessmentQuestion() { }

    public AssessmentQuestion(
        int assessmentId,
        string questionText,
        int questionOrder,
        int actionerId,
        string questionType)
    {
        AssessmentId = assessmentId;
        QuestionText = questionText;
        QuestionOrder = questionOrder;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
        QuestionType = questionType;
    }

    public void ApplyUpdate(string? questionText, int? questionOrder, int actionerId)
    {
        QuestionText = UseIfProvided(questionText, QuestionText);
        QuestionOrder = questionOrder ?? QuestionOrder;
        MarkAsModified(actionerId);
    }
}