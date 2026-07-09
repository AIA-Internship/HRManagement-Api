namespace HRManagement.Domain.Models.Payload;

public record SaveOrSubmitPeerReviewPayload
{
    public bool IsDraft { get; init; }
    public List<PeerReviewAssignmentPayload> PeerReviews { get; init; } = new();
}

public record PeerReviewAssignmentPayload
{
    public int AssignmentId { get; init; }
    public List<PeerReviewAnswerPayload> Answers { get; init; } = new();
}

public record PeerReviewAnswerPayload
{
    public int AssessmentQuestionId { get; init; }
    public string? TextValue { get; init; }
    public int? RatingValue { get; init; }
}