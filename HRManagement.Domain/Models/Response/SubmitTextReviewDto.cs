namespace HRManagement.Domain.Models.Payload;

public class SubmitTextReviewPayload
{
    public int PlanId { get; set; }
    public int IntervalId { get; set; }
    public int SubjectId { get; set; }
    public int ReviewerRoleId { get; set; }
    public string ReviewerJobTitle { get; set; } = string.Empty;
    public bool IsDraft { get; set; }
    public List<ReviewScoreItemPayload> ReviewScores { get; set; } = new();
}

public class ReviewScoreItemPayload
{
    public int AssessmentAnswerId { get; set; }
    public decimal Score { get; set; }
}