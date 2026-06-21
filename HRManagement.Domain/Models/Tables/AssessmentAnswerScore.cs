namespace HRManagement.Domain.Models.Tables;

public class AssessmentAnswerScore : BaseTable
{
    public int Id { get; private set; }
    public int AssessmentAnswerId { get; private set; }
    public decimal Score { get; private set; }
    public int ReviewerId { get; private set; }

    protected AssessmentAnswerScore() { }

    public AssessmentAnswerScore(
        int assessmentAnswerId,
        decimal score,
        int reviewerId,
        int actionerId)
    {
        AssessmentAnswerId = assessmentAnswerId;
        Score = score;
        ReviewerId = reviewerId;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(decimal? score, int? reviewerId, int actionerId)
    {
        Score = score ?? Score;
        ReviewerId = reviewerId ?? ReviewerId;
        MarkAsModified(actionerId);
    }
}