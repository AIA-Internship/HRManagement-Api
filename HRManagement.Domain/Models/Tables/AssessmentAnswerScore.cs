namespace HRManagement.Domain.Models.Tables;

public class AssessmentAnswerScore : BaseTable
{
    public int Id { get; private set; }
    public int AssessmentAnswerId { get; private set; }
    public decimal Score { get; private set; }
    public int ReviewerId { get; private set; }
    public int ReviewAssignmentId { get; private set; }
    public AssessmentAnswer AssessmentAnswer { get; private set; } = null!;

    protected AssessmentAnswerScore() { }

    public AssessmentAnswerScore(
        int reviewAssignmentId,
        int assessmentAnswerId,
        decimal score,
        int reviewerId,
        int actionerId)
    {
        ReviewAssignmentId = reviewAssignmentId;
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

    public void UpdateScore(decimal newScore, int? modifiedBy)
    {
        Score = newScore;
        MarkAsModified(modifiedBy ?? 0);
        ModifiedUtcDate = DateTime.UtcNow;
    }
}