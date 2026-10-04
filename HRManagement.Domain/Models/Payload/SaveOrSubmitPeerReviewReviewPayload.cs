namespace HRManagement.Domain.Models.Payload
{
    public class SaveOrSubmitPeerReviewReviewPayload
    {
        public int IntervalId { get; set; }
        public int SubjectId { get; set; }
        public bool IsDraft { get; set; }
        public List<PeerReviewScoreItemDto> ReviewScores { get; set; } = new();
    }

    public class PeerReviewScoreItemDto
    {
        public int AssessmentAnswerId { get; set; }
        public decimal Score { get; set; }
    }
}